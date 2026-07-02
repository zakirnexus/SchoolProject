using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.StudyAbroad;

namespace SchoolProject.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    [Route("admin/study-abroad/institutes")]
    public class AdminStudyAbroadInstituteController : Controller
    {
        private readonly AppDbContext _context;

        public AdminStudyAbroadInstituteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /admin/study-abroad/institutes
        [HttpGet("")]
        public IActionResult Index()
        {
            var institutes = _context.IntlInstitutes
                .Include(i => i.Country)
                .Include(i => i.City)
                .OrderBy(i => i.Country.CountryName)
                .ThenBy(i => i.InstituteName)
                .ToList();

            return View(institutes);
        }

        // GET: /admin/study-abroad/institutes/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View(new IntlInstitute
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // POST: /admin/study-abroad/institutes/create
        [HttpPost("create")]
        public IActionResult Create(IntlInstitute model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Slug))
            {
                model.Slug = GenerateSlug(model.InstituteName);
            }

            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            _context.IntlInstitutes.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Institute created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /admin/study-abroad/institutes/edit/{id}
        [HttpGet("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var institute = _context.IntlInstitutes.Find(id);
            if (institute == null)
                return NotFound();

            LoadDropdowns();
            return View(institute);
        }

        // POST: /admin/study-abroad/institutes/edit/{id}
        [HttpPost("edit/{id:int}")]
        public IActionResult Edit(int id, IntlInstitute model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(model);
            }

            var existing = _context.IntlInstitutes.Find(id);
            if (existing == null)
                return NotFound();

            existing.InstituteName = model.InstituteName;
            existing.InstituteNameAlt = model.InstituteNameAlt;
            existing.CountryId = model.CountryId;
            existing.CityId = model.CityId;
            existing.Address = model.Address;
            existing.Pincode = model.Pincode;
            existing.Locality = model.Locality;
            existing.UniversityName = model.UniversityName;
            existing.SyllabusAffiliation = model.SyllabusAffiliation;
            existing.InstituteStatus = model.InstituteStatus;
            existing.InstituteStatusSub = model.InstituteStatusSub;
            existing.InstituteLevel = model.InstituteLevel;
            existing.Telephone = model.Telephone;
            existing.Fax = model.Fax;
            existing.Email = model.Email;
            existing.Email2 = model.Email2;
            existing.Accreditation = model.Accreditation;
            existing.ApprovedBy = model.ApprovedBy;
            existing.InstituteRanking = model.InstituteRanking;
            existing.IndiaTodayRanking = model.IndiaTodayRanking;
            existing.WebsiteUrl = model.WebsiteUrl;
            existing.DescriptionHtml = model.DescriptionHtml;
            existing.CoEd = model.CoEd;
            existing.Keywords = model.Keywords;
            existing.LogoPath = model.LogoPath;
            existing.IsPaidListing = model.IsPaidListing;
            existing.Photos = model.Photos;
            existing.EstablishedYear = model.EstablishedYear;
            existing.MetaDescription = model.MetaDescription;
            existing.Consultants = model.Consultants;
            existing.Status = model.Status;

            if (string.IsNullOrWhiteSpace(existing.Slug))
            {
                existing.Slug = GenerateSlug(existing.InstituteName);
            }

            existing.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            TempData["Success"] = "Institute updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /admin/study-abroad/institutes/delete/{id}
        [HttpPost("delete/{id:int}")]
        public IActionResult Delete(int id)
        {
            var institute = _context.IntlInstitutes.Find(id);
            if (institute == null)
                return NotFound();

            // simple soft delete: mark Status or IsPaidListing, depending on your convention
            institute.Status = "Inactive";
            institute.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            TempData["Success"] = "Institute marked as inactive.";
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns()
        {
            ViewBag.Countries = _context.IntlCountries
                .Where(c => c.IsActive)
                .OrderBy(c => c.CountryName)
                .ToList();

            ViewBag.Cities = _context.IntlCities
                .Where(c => c.IsActive)
                .OrderBy(c => c.CityName)
                .ToList();
        }

        private string GenerateSlug(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            return text
                .Trim()
                .ToLower()
                .Replace("&", "and")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("/", "-")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(" ", "-")
                .Replace("--", "-");
        }
    }
}
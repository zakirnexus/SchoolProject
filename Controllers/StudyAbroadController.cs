using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.StudyAbroad;

namespace SchoolProject.Controllers
{
    public class StudyAbroadController : Controller
    {
        private readonly AppDbContext _db;

        public StudyAbroadController(AppDbContext db)
        {
            _db = db;
        }

        // /study-abroad
        [Route("study-abroad")]
        public async Task<IActionResult> Index()
        {
            var countries = await _db.IntlCountries
                .Where(c => c.IsActive)
                .OrderBy(c => c.CountryName)
                .ToListAsync();

            return View(countries);
        }

        // /study-abroad/{countrySlug}
        [Route("study-abroad/{countrySlug}")]
        public async Task<IActionResult> Country(string countrySlug)
        {
            if (string.IsNullOrWhiteSpace(countrySlug))
                return NotFound();

            var country = await _db.IntlCountries
                .Include(c => c.Institutes)
                .ThenInclude(i => i.City)
                .Where(c => c.IsActive)
                .FirstOrDefaultAsync(c => c.Slug == countrySlug);

            if (country == null)
                return NotFound();

            return View(country);
        }

        // /study-abroad/{countrySlug}/{instituteSlug}
        [Route("study-abroad/{countrySlug}/{instituteSlug}")]
        public async Task<IActionResult> Institute(string countrySlug, string instituteSlug)
        {
            if (string.IsNullOrWhiteSpace(countrySlug) || string.IsNullOrWhiteSpace(instituteSlug))
                return NotFound();

            var institute = await _db.IntlInstitutes
                .Include(i => i.Country)
                .Include(i => i.City)
                .Include(i => i.InstituteCourses)
                    .ThenInclude(ic => ic.Course)
                    .ThenInclude(c => c.Category)
                .Include(i => i.InstituteCourses)
                    .ThenInclude(ic => ic.Course)
                    .ThenInclude(c => c.Level)
                .Include(i => i.InstituteCourses)
                    .ThenInclude(ic => ic.Course)
                    .ThenInclude(c => c.CourseNameTemplate)   // <-- add this
                .FirstOrDefaultAsync(i => i.Slug == instituteSlug &&
                                        i.Country.Slug == countrySlug);

            if (institute == null)
                return NotFound();

            return View(institute);
        }
    }
}
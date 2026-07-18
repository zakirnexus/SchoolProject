using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Search;
using SchoolProject.Services.Search.Interfaces;

namespace SchoolProject.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;

        private readonly ISchoolSearchService _schoolSearchService;
        private readonly ICollegeSearchService _collegeSearchService;
        private readonly ICourseSearchService _courseSearchService;
        private readonly ISpecializationSearchService _specializationSearchService;

        public SearchController(
            AppDbContext context,
            ISchoolSearchService schoolSearchService,
            ICollegeSearchService collegeSearchService,
            ICourseSearchService courseSearchService,
            ISpecializationSearchService specializationSearchService)
        {
            _context = context;
            _schoolSearchService = schoolSearchService;
            _collegeSearchService = collegeSearchService;
            _courseSearchService = courseSearchService;
            _specializationSearchService = specializationSearchService;
        }

        public IActionResult Index(string q, int page = 1)
        {
            var results = new List<SearchResultViewModel>();

            if (string.IsNullOrWhiteSpace(q))
            {
                ViewBag.Query = "";

                ViewBag.ResultCount = 0;
                ViewBag.CollegeCount = 0;
                ViewBag.CourseCount = 0;
                ViewBag.SchoolCount = 0;

                return View("IndexV2", results);
            }

            q = q.Trim();

            var words = q.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Schools
            var schools = _schoolSearchService.Search(words);
            results.AddRange(schools);

            // Colleges
            var colleges = _collegeSearchService.Search(words);
            results.AddRange(colleges);

            // Courses
            var courses = _courseSearchService.Search(words);
            results.AddRange(courses);

            // Specializations
            var specializations = _specializationSearchService.Search(words);
            results.AddRange(specializations);

            const int PageSize = 25;

            results = results
                .GroupBy(r => r.Url)
                .Select(g =>
                {
                    // Prefer the result with the highest score.
                    // If scores tie, prefer the one with a richer description.
                    var best = g
                        .OrderByDescending(x => x.Score)
                        .ThenByDescending(x => x.Description?.Length ?? 0)
                        .First();

                    // CourseSearchService and SpecializationSearchService only
                    // populate Title/Url/Type/Description/Score - they don't set
                    // Logo, CampusImage, Address, etc. Since they can carry a
                    // higher Score than CollegeSearchService/SchoolSearchService,
                    // "best" above can end up being the sparse version even when
                    // a richer duplicate exists in the same group.
                    // Backfill the display fields from whichever duplicate
                    // actually has them (identified here by a non-zero InstituteId,
                    // since only CollegeSearchService/SchoolSearchService set it).
                    var richest = g.FirstOrDefault(x => x.InstituteId != 0) ?? best;

                    if (!ReferenceEquals(best, richest))
                    {
                        best.InstituteId = richest.InstituteId;
                        best.Logo ??= richest.Logo;
                        best.CampusImage ??= richest.CampusImage;
                        best.Address ??= richest.Address;
                        best.Website ??= richest.Website;
                        best.Phone ??= richest.Phone;
                        best.Ownership ??= richest.Ownership;
                        best.EstablishedYear ??= richest.EstablishedYear;

                        if (string.IsNullOrWhiteSpace(best.Accreditation))
                            best.Accreditation = richest.Accreditation;

                        best.Sponsored = best.Sponsored || richest.Sponsored;

                        if (best.ListingRank == 0)
                            best.ListingRank = richest.ListingRank;
                    }

                    return best;
                })
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Type)
                .ThenBy(r => r.Title)
                .ToList();

            var totalResults = results.Count;

            ViewBag.Query = q;
            ViewBag.ResultCount = totalResults;

            ViewBag.CollegeCount = results.Count(r => r.Type == "College");
            ViewBag.SchoolCount = results.Count(r => r.Type == "School");
            ViewBag.CourseCount = results.Count(r => r.Type == "Course");
            ViewBag.SpecializationCount = results.Count(r => r.Type == "Specialization");
            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalResults = totalResults;
            ViewBag.TotalPages = (int)Math.Ceiling(totalResults / (double)PageSize);

            var pagedResults = results
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return View("IndexV2", pagedResults);
        }

        [HttpGet]
        public IActionResult AutoComplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            term = term.Trim();

            var schoolResults = _context.Schools
                .Where(s =>
                    EF.Functions.Like(s.InstituteName, "%" + term + "%")
                )
                .Select(s => new
                {
                    label = s.InstituteName + " (School)",
                    value = s.InstituteName,
                    url = "/school/" + s.InstituteSlug
                })
                .Take(5)
                .ToList();

            var collegeResults = _context.Colleges
                .Where(c =>
                    EF.Functions.Like(c.InstituteName, "%" + term + "%")
                )
                .Select(c => new
                {
                    label = c.InstituteName + " (College)",
                    value = c.InstituteName,
                    url = "/college/" + c.InstituteSlug
                })
                .Take(5)
                .ToList();

            var courseResults = _context.Courses
                .Where(c =>
                    EF.Functions.Like(c.CourseName, "%" + term + "%")
                )
                .Select(c => new
                {
                    label = c.CourseName + " (Course)",
                    value = c.CourseName,
                    url = "/courses/" + c.CourseSlug
                })
                .Take(5)
                .ToList();

            var results = schoolResults
                .Concat(collegeResults)
                .Concat(courseResults)
                .Take(10)
                .ToList();

            return Json(results);
        }
    }
}

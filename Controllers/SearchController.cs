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
                return View(results);

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
                    return g
                        .OrderByDescending(x => x.Score)
                        .ThenByDescending(x => x.Description?.Length ?? 0)
                        .First();
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

            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalResults = totalResults;
            ViewBag.TotalPages = (int)Math.Ceiling(totalResults / (double)PageSize);

            var pagedResults = results
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return View(pagedResults);
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

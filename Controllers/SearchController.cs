using Microsoft.AspNetCore.Mvc;
using SchoolProject.Models.Search;
using SchoolProject.Services.Elasticsearch.Interfaces;

namespace SchoolProject.Controllers
{
    public class SearchController : Controller
    {
        private const int PageSize = 25;

        private readonly IElasticQueryService _elasticQueryService;

        public SearchController(IElasticQueryService elasticQueryService)
        {
            _elasticQueryService = elasticQueryService;
        }

        public async Task<IActionResult> Index(string q, int page = 1, CancellationToken cancellationToken = default)
        {
            q = (q ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(q))
            {
                ViewBag.Query = "";

                ViewBag.ResultCount = 0;
                ViewBag.CollegeCount = 0;
                ViewBag.CourseCount = 0;
                ViewBag.SchoolCount = 0;
                ViewBag.SpecializationCount = 0;

                return View("IndexV2", new List<SearchResultViewModel>());
            }

            page = page < 1 ? 1 : page;

            var request = new ElasticSearchRequest
            {
                Query = q,
                Page = page,
                Size = PageSize
            };

            // A single Elasticsearch query now does everything the old code
            // needed four separate SQL-based services (School/College/Course/
            // Specialization) plus a manual GroupBy-and-merge pass to
            // accomplish: full-text matching, relevance + boost ranking,
            // pagination, and per-type counts. The search index already has
            // one canonical, fully-populated document per entity, so there's
            // no duplicate-result merging to do here anymore.
            var result = await _elasticQueryService.SearchAsync(request, cancellationToken);

            ViewBag.Query = q;
            ViewBag.ResultCount = result.Total;

            ViewBag.CollegeCount = result.FacetCounts.GetValueOrDefault(nameof(SearchEntityType.College));
            ViewBag.SchoolCount = result.FacetCounts.GetValueOrDefault(nameof(SearchEntityType.School));
            ViewBag.CourseCount = result.FacetCounts.GetValueOrDefault(nameof(SearchEntityType.Course));
            ViewBag.SpecializationCount = result.FacetCounts.GetValueOrDefault(nameof(SearchEntityType.Specialization));

            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalResults = result.Total;
            ViewBag.TotalPages = (int)Math.Ceiling(result.Total / (double)PageSize);

            return View("IndexV2", result.Results);
        }

        [HttpGet]
        public async Task<IActionResult> AutoComplete(string term, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var suggestions = await _elasticQueryService.AutoCompleteAsync(term.Trim(), 10, cancellationToken);

            var results = suggestions.Select(s => new
            {
                label = s.Label,
                value = s.Value,
                url = s.Url
            });

            return Json(results);
        }
    }
}

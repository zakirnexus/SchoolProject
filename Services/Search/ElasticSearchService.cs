using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Search;

namespace SchoolProject.Services.Search
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly AppDbContext _context;

        public ElasticSearchService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<SearchResultViewModel>> SearchAsync(ElasticSearchRequest request, CancellationToken cancellationToken = default)
        {
            // Phase 1 stub: keep SQL fallback while ES client is wired.
            var q = (request.Query ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q)) return new List<SearchResultViewModel>();

            var words = q.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var schools = await _context.Schools
                .Include(s => s.City)
                .Where(s => s.IsActive)
                .Where(s => words.All(w =>
                    EF.Functions.Like(s.InstituteName!, "%" + w + "%") ||
                    (s.Keyword != null && EF.Functions.Like(s.Keyword, "%" + w + "%")) ||
                    (s.City != null && s.City.CityName != null && EF.Functions.Like(s.City.CityName, "%" + w + "%"))))
                .Select(s => new SearchResultViewModel
                {
                    Title = s.InstituteName ?? "",
                    Url = "/school/" + s.InstituteSlug,
                    Type = "School",
                    Description = s.Address
                })
                .Take(request.Size)
                .ToListAsync(cancellationToken);

            return schools;
        }

        public Task<IReadOnlyList<object>> AutoCompleteAsync(string term, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<object> empty = new List<object>();
            return Task.FromResult(empty);
        }

        public Task RebuildIndexAsync(CancellationToken cancellationToken = default)
        {
            // Phase 2: replace with actual indexing pipeline to ES.
            return Task.CompletedTask;
        }
    }
}

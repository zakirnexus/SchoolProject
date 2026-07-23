using SchoolProject.Models.Search;
using SchoolProject.Services.Elasticsearch.Interfaces;

namespace SchoolProject.Services.Search
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly IElasticQueryService _elasticQueryService;
        private readonly IElasticBulkIndexer _bulkIndexer;

        public ElasticSearchService(
            IElasticQueryService elasticQueryService,
            IElasticBulkIndexer bulkIndexer)
        {
            _elasticQueryService = elasticQueryService;
            _bulkIndexer = bulkIndexer;
        }

        public async Task<IReadOnlyList<SearchResultViewModel>> SearchAsync(
            ElasticSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Query))
                return new List<SearchResultViewModel>();

            var result = await _elasticQueryService.SearchAsync(
                request,
                cancellationToken);

            return result.Results;
        }

        public async Task<IReadOnlyList<object>> AutoCompleteAsync(
            string term,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<object>();

            var suggestions = await _elasticQueryService.AutoCompleteAsync(
                term,
                10,
                cancellationToken);

            return suggestions
                .Select(s => (object)new
                {
                    label = s.Label,
                    value = s.Value,
                    url = s.Url
                })
                .ToList();
        }

        public Task RebuildIndexAsync(
            CancellationToken cancellationToken = default)
        {
            return _bulkIndexer.RebuildIndexAsync(cancellationToken);
        }
    }
}

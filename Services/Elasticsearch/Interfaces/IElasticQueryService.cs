using SchoolProject.Models.Search;

namespace SchoolProject.Services.Elasticsearch.Interfaces
{
    public interface IElasticQueryService
    {
        /// <summary>
        /// Runs a filtered, paginated full-text search against the search index
        /// and returns the page of results plus total hit count and per-type
        /// facet counts.
        /// </summary>
        Task<SearchQueryResult> SearchAsync(
            ElasticSearchRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns lightweight type-ahead suggestions for a partial query term.
        /// </summary>
        Task<List<AutoCompleteResult>> AutoCompleteAsync(
            string term,
            int size = 10,
            CancellationToken cancellationToken = default);
    }
}

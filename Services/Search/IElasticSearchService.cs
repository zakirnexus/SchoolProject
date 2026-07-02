using SchoolProject.Models.Search;

namespace SchoolProject.Services.Search
{
    public interface IElasticSearchService
    {
        Task<IReadOnlyList<SearchResultViewModel>> SearchAsync(ElasticSearchRequest request, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<object>> AutoCompleteAsync(string term, CancellationToken cancellationToken = default);
        Task RebuildIndexAsync(CancellationToken cancellationToken = default);
    }
}

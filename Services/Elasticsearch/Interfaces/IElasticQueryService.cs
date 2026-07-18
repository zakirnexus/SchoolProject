using SchoolProject.Models.Search;

namespace SchoolProject.Services.Elasticsearch.Interfaces
{
    public interface IElasticQueryService
    {
        Task<List<SearchResultViewModel>> SearchAsync(
            string query,
            int size = 25,
            CancellationToken cancellationToken = default);
    }
}
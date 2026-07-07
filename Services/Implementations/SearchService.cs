using SchoolProject.Services.Search.Interfaces;
using SchoolProject.Services.Search.Models;

namespace SchoolProject.Services.Search.Implementations
{
    public class SearchService : ISearchService
    {
        public async Task<SearchResult> SearchAsync(SearchRequest request)
        {
            await Task.CompletedTask;

            return new SearchResult
            {
                Success = true,
                Query = request.Query
            };
        }
    }
}
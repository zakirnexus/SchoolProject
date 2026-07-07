using SchoolProject.Services.Search.Models;

namespace SchoolProject.Services.Search.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResult> SearchAsync(SearchRequest request);
    }
}
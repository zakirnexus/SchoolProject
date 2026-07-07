using SchoolProject.Models.Search;

namespace SchoolProject.Services.Search.Interfaces
{
    public interface ICourseSearchService
    {
        List<SearchResultViewModel> Search(string[] words);
    }
}
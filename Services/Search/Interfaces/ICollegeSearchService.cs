using SchoolProject.Models.Search;

namespace SchoolProject.Services.Search.Interfaces
{
    public interface ICollegeSearchService
    {
        List<SearchResultViewModel> Search(string[] words);
    }
}
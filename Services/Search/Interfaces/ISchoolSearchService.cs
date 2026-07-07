using SchoolProject.Models.Search;

namespace SchoolProject.Services.Search.Interfaces
{
    public interface ISchoolSearchService
    {
        List<SearchResultViewModel> Search(string[] words);
    }
}
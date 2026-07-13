using SchoolProject.Models.Search;

namespace SchoolProject.Services.Search.Interfaces
{
    public interface ISpecializationSearchService
    {
        List<SearchResultViewModel> Search(string[] words);
    }
}
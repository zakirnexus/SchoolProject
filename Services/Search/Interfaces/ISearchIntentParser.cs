using SchoolProject.Services.Search.Models;

namespace SchoolProject.Services.Search.Interfaces
{
    public interface ISearchIntentParser
    {
        SearchIntent Parse(string query);
    }
}
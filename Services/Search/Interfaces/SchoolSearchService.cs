using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Search;
using SchoolProject.Services.Search.Interfaces;

namespace SchoolProject.Services.Search.Implementations
{
    public class SchoolSearchService : ISchoolSearchService
    {
        private readonly AppDbContext _context;

        public SchoolSearchService(AppDbContext context)
        {
            _context = context;
        }

        public List<SearchResultViewModel> Search(string[] words)
        {
            return _context.Schools
                .Include(s => s.City)
                .Where(s =>
                    words.All(w =>
                        EF.Functions.Like(s.InstituteName, "%" + w + "%") ||
                        (s.Keyword != null &&
                         EF.Functions.Like(s.Keyword, "%" + w + "%")) ||
                        (s.City.CityName != null &&
                         EF.Functions.Like(s.City.CityName, "%" + w + "%"))
                    ))
                .Select(s => new SearchResultViewModel
                {
                    Title = s.InstituteName,
                    Url = "/school/" + s.InstituteSlug,
                    Type = "School",
                    Description = s.Address
                })
                .Take(20)
                .ToList();
        }
    }
}
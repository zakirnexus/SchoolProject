using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Search;
using SchoolProject.Services.Search.Interfaces;

namespace SchoolProject.Services.Search.Implementations
{
    public class CollegeSearchService : ICollegeSearchService
    {
        private readonly AppDbContext _context;

        public CollegeSearchService(AppDbContext context)
        {
            _context = context;
        }

        public List<SearchResultViewModel> Search(string[] words)
        {
            return _context.Colleges
                .Where(c =>
                    words.All(w =>
                        EF.Functions.Like(c.InstituteName, "%" + w + "%") ||
                        (c.Address != null &&
                         EF.Functions.Like(c.Address, "%" + w + "%"))
                    ))
                .Select(c => new SearchResultViewModel
                {
                    Title = c.InstituteName,
                    Url = "/college/" + c.InstituteSlug,
                    Type = "College",
                    Description = c.Address
                })
                .Take(20)
                .ToList();
        }
    }
}
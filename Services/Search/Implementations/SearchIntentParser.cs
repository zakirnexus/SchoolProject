using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Services.Search.Interfaces;
using SchoolProject.Services.Search.Models;

namespace SchoolProject.Services.Search.Implementations
{
    public class SearchIntentParser : ISearchIntentParser
    {
        private readonly AppDbContext _context;

        public SearchIntentParser(AppDbContext context)
        {
            _context = context;
        }

        public SearchIntent Parse(string query)
        {
            query = query.Trim();

            var aliases = _context.CourseAliases
                .AsNoTracking()
                .OrderByDescending(a => a.AliasName.Length)
                .ToList();

            foreach (var alias in aliases)
            {
                if (query.Contains(alias.AliasName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    var variant = query.Replace(
                        alias.AliasName,
                        "",
                        StringComparison.OrdinalIgnoreCase).Trim();

                    return new SearchIntent
                    {
                        CourseId = alias.CourseId,
                        CanonicalCourse = alias.AliasName,
                        Variant = variant
                    };
                }
            }

            return new SearchIntent();
        }
    }
}
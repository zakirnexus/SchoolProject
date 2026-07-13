using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Courses;
using SchoolProject.Services.Search.Interfaces;

namespace SchoolProject.Services.Search.Implementations
{
    public class CourseAliasResolver : ICourseAliasResolver
    {
        private readonly AppDbContext _context;

        public CourseAliasResolver(AppDbContext context)
        {
            _context = context;
        }

        public CourseAlias? ResolveAlias(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return null;

            searchText = searchText.Trim().ToLower();

            var alias = _context.CourseAliases
                .AsNoTracking()
                .Where(a => a.IsActive)
                .FirstOrDefault(a =>
                    a.AliasName.ToLower() == searchText);

            return alias;
        }

        public int? ResolveCourseId(string searchText)
        {
            var alias = ResolveAlias(searchText);
            return alias?.CourseId;
        }
    }
}
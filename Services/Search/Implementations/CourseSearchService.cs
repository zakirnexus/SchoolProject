using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Search;
using SchoolProject.Services.Search.Interfaces;

namespace SchoolProject.Services.Search.Implementations
{
    public class CourseSearchService : ICourseSearchService
    {
        private readonly AppDbContext _context;

        public CourseSearchService(AppDbContext context)
        {
            _context = context;
        }

        public List<SearchResultViewModel> Search(string[] words)
        {
            return _context.Colleges
                .Include(c => c.CollegeCourses)
                    .ThenInclude(cc => cc.Course)
                .Where(c =>
                    words.All(w =>

                        EF.Functions.Like(c.InstituteName, "%" + w + "%")

                        ||

                        (c.Address != null &&
                        EF.Functions.Like(c.Address, "%" + w + "%"))

                        ||

                        c.CollegeCourses!.Any(cc =>

                            cc.Course != null &&

                            EF.Functions.Like(
                                cc.Course.CourseName,
                                "%" + w + "%")

                        )
                    ))
                .Select(c => new SearchResultViewModel
                {
                    Title = c.InstituteName,
                    Url = "/college/" + c.InstituteSlug,
                    Type = "College",
                    Description = c.Address
                })
                .Distinct()
                .Take(20)
                .ToList();
        }
    }
}
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
            var query = from college in _context.Colleges
                        join cc in _context.CollegeCourses
                            on college.InstituteId equals cc.InstituteId
                        join course in _context.Courses
                            on cc.CourseId equals course.CourseId
                        where words.All(w =>
                            EF.Functions.Like(course.CourseName, "%" + w + "%"))
                        select new SearchResultViewModel
                        {
                            Title = college.InstituteName,
                            Url = "/college/" + college.InstituteSlug,
                            Type = "College",
                            Description = college.Address
                        };

            return query
                .Distinct()
                .Take(20)
                .ToList();
        }
    }
}
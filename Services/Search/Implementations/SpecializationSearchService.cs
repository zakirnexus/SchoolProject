using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Search;
using SchoolProject.Services.Search.Interfaces;

namespace SchoolProject.Services.Search.Implementations
{
    public class SpecializationSearchService : ISpecializationSearchService
    {
        private readonly AppDbContext _context;

        public SpecializationSearchService(AppDbContext context)
        {
            _context = context;
        }

        public List<SearchResultViewModel> Search(string[] words)
        {
            var query =
                from college in _context.Colleges

                join cc in _context.CollegeCourses
                    on college.InstituteId equals cc.InstituteId

                join course in _context.Courses
                    on cc.CourseId equals course.CourseId

                join sp in _context.Specializations
                    on cc.SpecializationId equals sp.SpecializationId
                    into spJoin

                from sp in spJoin.DefaultIfEmpty()

                where words.All(w =>

                    (sp != null &&
                    sp.SpecializationName != null &&
                    EF.Functions.Like(sp.SpecializationName, "%" + w + "%"))

                    ||

                    (cc.OriginalCourseName != null &&
                    EF.Functions.Like(cc.OriginalCourseName, "%" + w + "%"))

                    ||

                    (course.CourseName != null &&
                    EF.Functions.Like(course.CourseName, "%" + w + "%"))

                )

                select new SearchResultViewModel
                {
                    Title = college.InstituteName,

                    Url = "/college/" + college.InstituteSlug,

                    Type = "College",

                    Description =
                        sp != null
                            ? sp.SpecializationName
                            : cc.OriginalCourseName ?? course.CourseName,

                    Score = 200
                };

            return query
                .AsEnumerable()
                .GroupBy(x => x.Url)
                .Select(g =>
                {
                    var descriptions = g.Select(x => x.Description)
                        .Where(d => !string.IsNullOrWhiteSpace(d))
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList();

                    return new SearchResultViewModel
                    {
                        Title = g.First().Title,
                        Url = g.First().Url,
                        Type = g.First().Type,

                        Description =
                            descriptions.Count <= 3
                                ? string.Join("<br/>", descriptions)
                                : string.Join("<br/>", descriptions.Take(3))
                                    + $"<br/><strong>+ {descriptions.Count - 3} more...</strong>",

                        Score = g.Max(x => x.Score)
                    };
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Title)
                .ToList();
        }
    }
}
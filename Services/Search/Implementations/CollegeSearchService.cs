using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Search;
using SchoolProject.Services.Search.Interfaces;

namespace SchoolProject.Services.Search.Implementations
{
    public class CollegeSearchService : ICollegeSearchService
    {
        private readonly AppDbContext _context;
        private readonly ISearchIntentParser _searchIntentParser;
private readonly IQueryNormalizer _queryNormalizer;

        public CollegeSearchService(
            AppDbContext context,
            ISearchIntentParser searchIntentParser,
            IQueryNormalizer queryNormalizer)
        {
            _context = context;
            _searchIntentParser = searchIntentParser;
            _queryNormalizer = queryNormalizer;
        }

        public List<SearchResultViewModel> Search(string[] words)
        {
            var query = string.Join(" ", words);

            var intent = _searchIntentParser.Parse(query);

            if (intent.IsCourseSearch)
            {
                var collegeQuery =
                    from college in _context.Colleges
                    join cc in _context.CollegeCourses
                        on college.InstituteId equals cc.InstituteId
                    where cc.CourseId == intent.CourseId
                    select new
                    {
                        College = college,
                        Course = cc
                    };

                if (!string.IsNullOrWhiteSpace(intent.Variant))
                {
                    var keywords = _queryNormalizer.Normalize(intent.Variant);

                    foreach (var keyword in keywords)
                    {
                        var word = keyword;

                        collegeQuery = collegeQuery.Where(x =>
                            x.Course.OriginalCourseName != null &&
                            EF.Functions.Like(
                                x.Course.OriginalCourseName,
                                "%" + word + "%"));
                    }
                }

                return collegeQuery
                    .Select(x => new SearchResultViewModel
                    {
                        Title = x.College.InstituteName,
                        Url = "/college/" + x.College.InstituteSlug,
                        Type = "College",
                        Description = x.College.Address,
                        Score = 100
                    })
                    .Distinct()
                    .OrderByDescending(x => x.Score)
                    .ThenBy(x => x.Title)
                    .ToList();
            }

            return _context.Colleges
                .Where(c =>
                    words.All(w =>
                        EF.Functions.Like(c.InstituteName, "%" + w + "%") ||
                        (c.Address != null &&
                        EF.Functions.Like(c.Address, "%" + w + "%"))))
                .Select(c => new SearchResultViewModel
                {
                    Title = c.InstituteName,
                    Url = "/college/" + c.InstituteSlug,
                    Type = "College",
                    Description = c.Address,
                    Score = 10
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Title)
                .ToList();
        }
    }
}
using SchoolProject.Core.Discovery;
using SchoolProject.Data;

namespace SchoolProject.Services.Education.Resolvers
{
    public class CourseResolver : ICourseResolver
    {
        private readonly AppDbContext _context;

        public CourseResolver(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DiscoveryContext> ResolveAsync(
            string courseSlug,
            string citySlug)
        {
            await Task.CompletedTask;

            return new DiscoveryContext
            {
                CourseSlug = courseSlug,
                CitySlug = citySlug,
                Success = false
            };
        }
    }
}
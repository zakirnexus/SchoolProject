using SchoolProject.Core.Discovery;

namespace SchoolProject.Services.Education.Resolvers
{
    public interface ICourseResolver
    {
        Task<DiscoveryContext> ResolveAsync(
            string courseSlug,
            string citySlug);
    }
}
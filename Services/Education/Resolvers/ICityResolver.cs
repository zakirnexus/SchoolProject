using SchoolProject.Models.Lookups;

namespace SchoolProject.Services.Education.Resolvers
{
    public interface ICityResolver
    {
        Task<City?> ResolveAsync(string citySlug);
    }
}
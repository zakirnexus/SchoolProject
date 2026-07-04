using SchoolProject.ViewModels.Education.Resolvers;

namespace SchoolProject.Services.Education.Resolvers
{
    public interface IListingCourseResolver
    {
        Task<CourseResolutionResult> ResolveAsync(
            string slug,
            bool isCoaching);
    }
}
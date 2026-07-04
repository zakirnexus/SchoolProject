using SchoolProject.Models.Courses;

namespace SchoolProject.ViewModels.Education.Resolvers
{
    public class CourseResolutionResult
    {
        public bool Success { get; init; }

        public bool IsCategory { get; init; }

        public Course? Course { get; init; }

        public CourseCategory? Category { get; init; }

        public int? SpecializationId { get; init; }

        public string DisplayName { get; init; } = "";

        public string RequestedSlug { get; init; } = "";
    }
}
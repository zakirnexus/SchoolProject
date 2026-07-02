using SchoolProject.Models.Courses;
using SchoolProject.Models.Lookups;

namespace SchoolProject.Core.Discovery
{
    public class DiscoveryContext
    {
        // Request
        public string CourseSlug { get; set; } = "";
        public string CitySlug { get; set; } = "";

        // Resolved Objects
        public Course? Course { get; set; }

        public CourseCategory? Category { get; set; }

        public City? City { get; set; }

        // Optional
        public int? SpecializationId { get; set; }

        // Request Type
        public bool IsCourse { get; set; }

        public bool IsCategory { get; set; }

        public bool IsCoaching { get; set; }

        // UI
        public string DisplayName { get; set; } = "";

        // Result
        public bool Success { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
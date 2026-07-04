using SchoolProject.Models.Colleges;
using SchoolProject.Models.Courses;

namespace SchoolProject.ViewModels.Education
{
    /// <summary>
    /// Represents the result returned by the Listing Service.
    /// This class contains everything required by the MVC controller
    /// to render a listing page.
    /// </summary>
    public class CollegeListingResult
    {
        /// <summary>
        /// Indicates whether the request was successfully resolved.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Indicates whether the resolved slug represents a course category.
        /// </summary>
        public bool IsCategory { get; set; }

        /// <summary>
        /// The resolved course, if applicable.
        /// </summary>
        public Course? Course { get; set; }

        /// <summary>
        /// The resolved course category, if applicable.
        /// </summary>
        public CourseCategory? Category { get; set; }

        /// <summary>
        /// The resolved specialization, if applicable.
        /// </summary>
        public int? SpecializationId { get; set; }

        /// <summary>
        /// Human-readable display name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Resolved city id.
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// Resolved city name.
        /// </summary>
        public string CityName { get; set; } = string.Empty;

        /// <summary>
        /// Collection of colleges returned by the listing.
        /// </summary>
        public List<College> Colleges { get; set; } = new();

        /// <summary>
        /// Total number of matching colleges.
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Current page number.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Number of records per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total page count.
        /// </summary>
        public int TotalPages =>
            PageSize == 0
                ? 0
                : (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}
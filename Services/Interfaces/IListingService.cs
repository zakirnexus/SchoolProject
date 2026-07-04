using SchoolProject.ViewModels.Education;

namespace SchoolProject.Services.Interfaces
{
    /// <summary>
    /// Provides education listing functionality for colleges and coaching.
    /// This is the primary application service used by MVC controllers.
    /// </summary>
    public interface IListingService
    {
        /// <summary>
        /// Retrieves a complete college/coaching listing.
        /// </summary>
        Task<CollegeListingResult> GetCollegeListingAsync(
            CollegeListingRequest request);
    }
}
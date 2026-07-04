using SchoolProject.Services.Education.Resolvers;
using SchoolProject.Services.Interfaces;
using SchoolProject.ViewModels.Education;

namespace SchoolProject.Services.Implementations
{
    /// <summary>
    /// Application service responsible for orchestrating
    /// college and coaching listings.
    /// </summary>
    public class ListingService : IListingService
    {
        private readonly IListingCourseResolver _listingCourseResolver;
        private readonly ICityResolver _cityResolver;

        public ListingService(
            IListingCourseResolver listingCourseResolver,
            ICityResolver cityResolver)
        {
            _listingCourseResolver = listingCourseResolver;
            _cityResolver = cityResolver;
        }

        public async Task<CollegeListingResult> GetCollegeListingAsync(
            CollegeListingRequest request)
        {
            var result = new CollegeListingResult();

            // Resolve course/category
            var courseResolution =
                await _listingCourseResolver.ResolveAsync(
                    request.Course,
                    request.IsCoaching);

            if (!courseResolution.Success)
                return result;

            result.Success = true;
            result.Course = courseResolution.Course;
            result.Category = courseResolution.Category;
            result.IsCategory = courseResolution.IsCategory;
            result.SpecializationId = courseResolution.SpecializationId;
            result.DisplayName = courseResolution.DisplayName;

            // Resolve city
            var city = await _cityResolver.ResolveAsync(request.City);

            if (city == null)
            {
                result.Success = false;
                return result;
            }

            result.CityId = city.CityId;
            result.CityName = city.CityName;

            return result;
        }
    }
}
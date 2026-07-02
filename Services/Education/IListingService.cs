using SchoolProject.Core.Results;
using SchoolProject.ViewModels.Education;

namespace SchoolProject.Services.Education
{
    public interface IListingService
    {
        Task<ServiceResult<EducationListingViewModel>> GetListingAsync(
            string courseSlug,
            string citySlug,
            int page,
            int? localityId,
            string? nsewc,
            int? coedId,
            int? ownershipId,
            string? feesRange);
    }
}
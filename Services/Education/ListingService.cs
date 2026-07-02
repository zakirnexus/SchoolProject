using SchoolProject.Core.Results;
using SchoolProject.Data;
using SchoolProject.ViewModels.Education;

namespace SchoolProject.Services.Education
{
    public class ListingService : IListingService
    {
        private readonly AppDbContext _context;

        public ListingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<EducationListingViewModel>> GetListingAsync(
            string courseSlug,
            string citySlug,
            int page,
            int? localityId,
            string? nsewc,
            int? coedId,
            int? ownershipId,
            string? feesRange)
        {
            await Task.CompletedTask;

            return ServiceResult<EducationListingViewModel>.Fail(
                "ListingService has not been implemented yet.");
        }
    }
}
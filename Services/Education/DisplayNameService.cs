using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;

namespace SchoolProject.Services.Education
{
    public class DisplayNameService : IDisplayNameService
    {
        private readonly AppDbContext _context;

        public DisplayNameService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetCourseDisplayNameAsync(
            int courseId,
            string courseName,
            int? specializationId)
        {
            if (!specializationId.HasValue)
                return courseName;

            var specialization = await _context.Specializations
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.SpecializationId == specializationId.Value &&
                    s.CourseId == courseId);

            if (specialization == null)
                return courseName;

            return $"{courseName} {specialization.SpecializationName}";
        }
    }
}
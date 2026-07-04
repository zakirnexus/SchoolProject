namespace SchoolProject.Services.Education
{
    public interface IDisplayNameService
    {
        Task<string> GetCourseDisplayNameAsync(
            int courseId,
            string courseName,
            int? specializationId);
    }
}
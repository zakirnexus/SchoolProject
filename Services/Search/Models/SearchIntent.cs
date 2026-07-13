namespace SchoolProject.Services.Search.Models
{
    public class SearchIntent
    {
        public int? CourseId { get; set; }

        public string CanonicalCourse { get; set; } = "";

        public string Variant { get; set; } = "";

        public bool IsCourseSearch => CourseId.HasValue;
    }
}
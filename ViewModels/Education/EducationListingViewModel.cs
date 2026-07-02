using SchoolProject.Models.Colleges;

namespace SchoolProject.ViewModels.Education
{
    public class EducationListingViewModel
    {
        public List<College> Providers { get; set; } = new();

        public string CourseName { get; set; } = "";

        public string CourseSlug { get; set; } = "";

        public string CityName { get; set; } = "";

        public string CitySlug { get; set; } = "";

        public int CourseId { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int TotalRecords { get; set; }

        public bool FiltersActive { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public string BottomContent { get; set; } = "";
    }
}
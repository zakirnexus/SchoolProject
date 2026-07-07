namespace SchoolProject.Services.Search.Models
{
    public class SearchRequest
    {
        public string Query { get; set; } = string.Empty;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public bool IncludeColleges { get; set; } = true;

        public bool IncludeSchools { get; set; } = true;

        public bool IncludeCourses { get; set; } = true;

        public bool IncludeCoaching { get; set; } = true;
    }
}
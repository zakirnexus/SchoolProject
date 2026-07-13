namespace SchoolProject.Models.Search
{
    public class SearchResultViewModel
    {
        // Core
        public int InstituteId { get; set; }

        public string Title { get; set; } = "";

        public string Url { get; set; } = "";

        public string Type { get; set; } = "";

        public int Score { get; set; }

        // College Information
        public string? Logo { get; set; }

        public string? CampusImage { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? Ownership { get; set; }

        public int? EstablishedYear { get; set; }

        public bool Sponsored { get; set; }

        public int ListingRank { get; set; }

        // Search Information
        public string? Description { get; set; }

        public string MatchReason { get; set; } = "";

        public List<string> MatchingCourses { get; set; } = new();

        public string? Accreditation { get; set; }

        public string? Phone { get; set; }

        public string? Website { get; set; }
    }
}
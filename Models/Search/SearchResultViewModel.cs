namespace SchoolProject.Models.Search
{
    public class SearchResultViewModel
    {
        // Identity
        public int InstituteId { get; set; }

        public string Title { get; set; } = "";

        public string Url { get; set; } = "";

        public string Type { get; set; } = "";

        // Rich UI

        public string? Logo { get; set; }

        public string? CampusImage { get; set; }

        public string? Address { get; set; }

        public string? Ownership { get; set; }

        public int? EstablishedYear { get; set; }

        public string? Accreditation { get; set; }

        public string? Website { get; set; }

        public string? Phone { get; set; }

        // Search

        public List<string> MatchingCourses { get; set; }
            = new();

        public string MatchReason { get; set; } = "";

        public int Score { get; set; }

        // SEO

        public string Description { get; set; } = "";

        // Flags

        public bool Sponsored { get; set; }

        public bool Featured { get; set; }

        public int ListingRank { get; set; }

        // Future

        public int? PlacementPercentage { get; set; }

        public bool HostelAvailable { get; set; }

        public bool WifiCampus { get; set; }

        public bool ScholarshipAvailable { get; set; }
    }
}
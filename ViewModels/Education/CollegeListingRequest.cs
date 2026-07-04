namespace SchoolProject.ViewModels.Education
{
    /// <summary>
    /// Represents a request for retrieving a college or coaching listing.
    /// This object encapsulates all request parameters coming from the route
    /// and query string, allowing controllers to pass a single object into
    /// the service layer.
    /// </summary>
    public class CollegeListingRequest
    {
        /// <summary>
        /// Course slug from the route.
        /// Example: mba, engineering, gmat
        /// </summary>
        public string Course { get; set; } = string.Empty;

        /// <summary>
        /// City slug from the route.
        /// Example: bangalore, mysore
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Current page number.
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Optional locality filter.
        /// </summary>
        public string? Locality { get; set; }

        /// <summary>
        /// Optional North/South/East/West/Central filter.
        /// </summary>
        public string? Nsewc { get; set; }

        /// <summary>
        /// Optional Co-education filter.
        /// </summary>
        public int? CoedId { get; set; }

        /// <summary>
        /// Optional Ownership filter.
        /// </summary>
        public int? OwnershipId { get; set; }

        /// <summary>
        /// Optional Fees Range filter.
        /// </summary>
        public string? FeesRange { get; set; }

        /// <summary>
        /// Indicates whether this request is for coaching listings.
        /// </summary>
        public bool IsCoaching { get; set; }
    }
}
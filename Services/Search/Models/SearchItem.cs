namespace SchoolProject.Services.Search.Models
{
    public class SearchItem
    {
        public string Title { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal? Rating { get; set; }

        public int Rank { get; set; }
    }
}
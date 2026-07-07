using System.Collections.Generic;

namespace SchoolProject.Services.Search.Models
{
    public class SearchResult
    {
        public bool Success { get; set; }

        public string Query { get; set; } = string.Empty;

        public int TotalResults { get; set; }

        public List<SearchItem> Results { get; set; } = new();
    }
}
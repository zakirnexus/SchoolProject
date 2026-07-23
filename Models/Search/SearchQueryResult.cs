namespace SchoolProject.Models.Search
{
    /// <summary>
    /// Wraps a single Elasticsearch search call: the page of results actually
    /// requested, the total number of matching documents across the whole
    /// index (for pagination), and per-entity-type counts (for the
    /// School/College/Course/Specialization tab counters on the results page).
    /// </summary>
    public class SearchQueryResult
    {
        public List<SearchResultViewModel> Results { get; set; } = new();

        public long Total { get; set; }

        /// <summary>
        /// Counts keyed by SearchEntityType name (e.g. "College", "School").
        /// </summary>
        public Dictionary<string, long> FacetCounts { get; set; } = new();
    }
}

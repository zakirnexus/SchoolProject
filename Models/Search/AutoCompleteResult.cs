namespace SchoolProject.Models.Search
{
    /// <summary>
    /// A single type-ahead suggestion returned by ElasticQueryService.AutoCompleteAsync.
    /// </summary>
    public class AutoCompleteResult
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string Url { get; set; } = "";
        public string Type { get; set; } = "";
    }
}

namespace SchoolProject.Config
{
    public class ElasticsearchOptions
    {
        public string Url { get; set; } = "http://localhost:9200";
        public string IndexName { get; set; } = "be_search_v1";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool Enabled { get; set; } = true;
    }
}

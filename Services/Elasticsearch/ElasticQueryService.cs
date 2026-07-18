using Elastic.Clients.Elasticsearch;
using SchoolProject.Models.Search;
using SchoolProject.Services.Elasticsearch.Interfaces;

namespace SchoolProject.Services.Elasticsearch
{
    public class ElasticQueryService : IElasticQueryService
    {
        private const string IndexName = "be_search_v1";

        private readonly ElasticsearchClient _client;

        public ElasticQueryService(ElasticsearchClient client)
        {
            _client = client;
        }

        public async Task<List<SearchResultViewModel>> SearchAsync(
            string query,
            int size = 25,
            CancellationToken cancellationToken = default)
        {
            query = (query ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(query))
                return new List<SearchResultViewModel>();

            var response = await _client.SearchAsync<ElasticSearchDocument>(
                s => s
                    .Index(IndexName)
                    .Size(size)
                    .Query(q => q
                        .MultiMatch(mm => mm
                            .Query(query)
                            .Fields("title,searchText,keywords,courseNames,specializationNames,cityName,localityName,description")
                        )
                    ),
                cancellationToken);

            if (!response.IsValidResponse || response.Documents is null)
                return new List<SearchResultViewModel>();

            return response.Documents
                .Select(d => new SearchResultViewModel
                {
                    InstituteId = d.EntityId,
                    Title = d.Title ?? string.Empty,
                    Url = d.Url ?? string.Empty,
                    Type = d.EntityType.ToString(),

                    Logo = d.Logo,
                    CampusImage = d.CampusImage,
                    Address = d.Address,
                    Ownership = d.OwnershipName,
                    EstablishedYear = d.EstablishedYear,
                    Accreditation = d.Accreditation,
                    Website = d.Website,
                    Phone = d.Phone,

                    MatchingCourses = d.CourseNames ?? new List<string>(),
                    MatchReason = "Elasticsearch",

                    Description = d.Description ?? d.MetaDescription ?? string.Empty,

                    Sponsored = d.IsSponsored,
                    Featured = d.IsFeatured,
                    ListingRank = d.ListingRank ?? 0,

                    PlacementPercentage = d.PlacementPercentage,
                    HostelAvailable = d.HostelAvailable,
                    WifiCampus = d.WifiCampus,
                    ScholarshipAvailable = d.ScholarshipAvailable,

                    Score = (int)Math.Round(d.BoostScore)
                })
                .ToList();
        }
    }
}
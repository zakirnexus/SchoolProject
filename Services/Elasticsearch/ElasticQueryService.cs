using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using SchoolProject.Models.Search;
using SchoolProject.Services.Elasticsearch.Interfaces;

namespace SchoolProject.Services.Elasticsearch
{
    public class ElasticQueryService : IElasticQueryService
    {
        private const string IndexName = "be_search_v1";

        // Field-boost weights for the main multi_match query. Title matches
        // matter most, then the aggregated SearchText/keyword blobs, then
        // location context. Uses Elasticsearch's native "field^weight" syntax.
        private static readonly string[] SearchFields =
        {
            "title^4",
            "searchText^2",
            "keywords^2",
            "courseNames^2",
            "specializationNames^2",
            "cityName",
            "localityName",
            "description"
        };

        private readonly ElasticsearchClient _client;
        private readonly ILogger<ElasticQueryService> _logger;

        public ElasticQueryService(
            ElasticsearchClient client,
            ILogger<ElasticQueryService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<SearchQueryResult> SearchAsync(
            ElasticSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            var queryText = (request.Query ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(queryText))
                return new SearchQueryResult();

            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.Size <= 0 ? 20 : Math.Min(request.Size, 100);
            var from = (page - 1) * pageSize;

            // Always-on filters plus whatever optional facet filters were
            // supplied on the request. Built as a list of filter actions so
            // we can add to it conditionally before handing it to .Filter().
            var filters = new List<Action<QueryDescriptor<ElasticSearchDocument>>>
            {
                f => f.Term(t => t.Field("isActive").Value(true))
            };

            if (!string.IsNullOrWhiteSpace(request.DocType) &&
                Enum.TryParse<SearchEntityType>(request.DocType, ignoreCase: true, out var entityType))
            {
                filters.Add(f => f.Term(t => t.Field("entityType").Value(entityType.ToString())));
            }

            if (!string.IsNullOrWhiteSpace(request.CitySlug))
                filters.Add(f => f.Term(t => t.Field("citySlug.keyword").Value(request.CitySlug)));

            if (!string.IsNullOrWhiteSpace(request.LocalitySlug))
                filters.Add(f => f.Term(t => t.Field("localitySlug.keyword").Value(request.LocalitySlug)));

            if (!string.IsNullOrWhiteSpace(request.SyllabusSlug))
                filters.Add(f => f.Term(t => t.Field("syllabusSlugs.keyword").Value(request.SyllabusSlug)));

            if (!string.IsNullOrWhiteSpace(request.NsewcName))
                filters.Add(f => f.Term(t => t.Field("nsewcName.keyword").Value(request.NsewcName)));

            try
            {
                var response = await _client.SearchAsync<ElasticSearchDocument>(s => s
                    .Index(IndexName)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q.Bool(b =>
                    {
                        b.Filter(filters.ToArray());

                        b.Must(m => m.MultiMatch(mm => mm
                            .Query(queryText)
                            .Fields(SearchFields)
                            .Type(TextQueryType.BestFields)
                            .Fuzziness(new Fuzziness("AUTO"))
                        ));
                    }))
                    .Sort(so => so
                        .Score(sc => sc.Order(SortOrder.Desc))
                        .Field("boostScore", fs => fs.Order(SortOrder.Desc)))
                    .Aggregations(a => a
                        .Add("by_type", agg => agg
                            .Terms(t => t.Field("entityType").Size(10)))),
                    cancellationToken);

                if (!response.IsValidResponse)
                {
                    _logger.LogWarning(
                        "Elasticsearch search returned an invalid response for query '{Query}'. Error: {Error}. Debug: {Debug}",
                        queryText,
                        response.ElasticsearchServerError?.ToString(),
                        response.DebugInformation);

                    return new SearchQueryResult();
                }

                var results = response.Documents
                    .Select(MapToViewModel)
                    .ToList();

                var facetCounts = new Dictionary<string, long>();
                var byType = response.Aggregations?.GetStringTerms("by_type");

                if (byType != null)
                {
                    foreach (var bucket in byType.Buckets)
                    {
                        var key = bucket.Key.ToString();
                        if (!string.IsNullOrWhiteSpace(key))
                            facetCounts[key] = bucket.DocCount;
                    }
                }
                else
                {
                    // entityType may have been mapped as a numeric/long field
                    // rather than a keyword depending on how the index was
                    // first created; degrade gracefully instead of throwing.
                    _logger.LogDebug(
                        "by_type aggregation did not return string terms buckets; facet counts unavailable for query '{Query}'.",
                        queryText);
                }

                return new SearchQueryResult
                {
                    Results = results,
                    Total = response.Total,
                    FacetCounts = facetCounts
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elasticsearch search threw for query '{Query}'.", queryText);
                return new SearchQueryResult();
            }
        }

        public async Task<List<AutoCompleteResult>> AutoCompleteAsync(
            string term,
            int size = 10,
            CancellationToken cancellationToken = default)
        {
            term = (term ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(term))
                return new List<AutoCompleteResult>();

            size = size <= 0 ? 10 : Math.Min(size, 20);

            try
            {
                var response = await _client.SearchAsync<ElasticSearchDocument>(s => s
                    .Index(IndexName)
                    .Size(size)
                    .Query(q => q.Bool(b => b
                        .Filter(
                            f => f.Term(t => t.Field("isActive").Value(true))
                        )
                        .Must(m => m.Bool(inner => inner
                            .Should(
                                sh => sh.MatchPhrasePrefix(mp => mp
                                    .Field("title")
                                    .Query(term)
                                    .Boost(8)),
                                sh => sh.Match(mt => mt
                                    .Field("title")
                                    .Query(term)
                                    .Boost(6)),
                                sh => sh.Match(mt => mt
                                    .Field("keywords")
                                    .Query(term)
                                    .Boost(3)),
                                sh => sh.Match(mt => mt
                                    .Field("searchText")
                                    .Query(term)
                                    .Boost(1))
                            )
                            .MinimumShouldMatch(1)
                        ))
                    ))
                    .Sort(so => so
                        .Score(sc => sc.Order(SortOrder.Desc))
                        .Field("boostScore", fs => fs.Order(SortOrder.Desc))),
                    cancellationToken);

                if (!response.IsValidResponse)
                {
                    _logger.LogWarning(
                        "Elasticsearch autocomplete returned an invalid response for term '{Term}'. Error: {Error}",
                        term,
                        response.ElasticsearchServerError?.ToString());

                    return new List<AutoCompleteResult>();
                }

                return response.Documents
                    .Where(d => !string.IsNullOrWhiteSpace(d.Title))
                    .GroupBy(d => d.Title)
                    .Select(g => g.First())
                    .Take(size)
                    .Select(d => new AutoCompleteResult
                    {
                        Label = $"{d.Title} ({d.EntityType})",
                        Value = d.Title ?? string.Empty,
                        Url = d.Url ?? string.Empty,
                        Type = d.EntityType.ToString()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elasticsearch autocomplete threw for term '{Term}'.", term);
                return new List<AutoCompleteResult>();
            }
        }

        private static SearchResultViewModel MapToViewModel(ElasticSearchDocument d)
        {
            return new SearchResultViewModel
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
            };
        }
    }
}

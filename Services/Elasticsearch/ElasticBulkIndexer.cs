using Elastic.Clients.Elasticsearch;
using SchoolProject.Models.Search;
using SchoolProject.Services.Elasticsearch.Interfaces;
using SchoolProject.Services.Search;

namespace SchoolProject.Services.Elasticsearch
{
    public class ElasticBulkIndexer : IElasticBulkIndexer
    {
        private const string IndexName = "be_search_v1";

        private readonly ElasticsearchClient _client;
        private readonly SearchIndexBuilder _builder;

        public ElasticBulkIndexer(
            ElasticsearchClient client,
            SearchIndexBuilder builder)
        {
            _client = client;
            _builder = builder;
        }

        public async Task RebuildIndexAsync(
            CancellationToken cancellationToken = default)
        {
            await DeleteIndexAsync(cancellationToken);
            await CreateIndexAsync(cancellationToken);
            await IndexSchoolsAsync(cancellationToken);
            await IndexCollegesAsync(cancellationToken);
        }

        public async Task DeleteIndexAsync(
            CancellationToken cancellationToken = default)
        {
            var exists = await _client.Indices.ExistsAsync(
                IndexName,
                cancellationToken);

            if (exists.Exists)
            {
                await _client.Indices.DeleteAsync(
                    IndexName,
                    cancellationToken);
            }
        }

        public async Task CreateIndexAsync(
            CancellationToken cancellationToken = default)
        {
            await _client.Indices.CreateAsync(
                IndexName,
                cancellationToken);
        }

        public async Task IndexSchoolsAsync(
            CancellationToken cancellationToken = default)
        {
            await BulkIndexAsync(
                SearchEntityType.School,
                cancellationToken);
        }

        public async Task IndexCollegesAsync(
            CancellationToken cancellationToken = default)
        {
            await BulkIndexAsync(
                SearchEntityType.College,
                cancellationToken);
        }

        private async Task BulkIndexAsync(
            SearchEntityType entityType,
            CancellationToken cancellationToken)
        {
            var docs = await _builder.BuildDocumentsAsync(cancellationToken);

            docs = docs
                .Where(x => x.EntityType == entityType)
                .ToList();

            if (docs.Count == 0)
                return;

            foreach (var doc in docs)
            {
                await _client.IndexAsync(
                    doc,
                    idx => idx
                        .Index(IndexName)
                        .Id(doc.Id),
                    cancellationToken);
            }

            await _client.Indices.RefreshAsync(
                IndexName,
                cancellationToken);
        }
    }
}
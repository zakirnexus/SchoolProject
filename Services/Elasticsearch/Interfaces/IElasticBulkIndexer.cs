using System.Threading;
using System.Threading.Tasks;

namespace SchoolProject.Services.Elasticsearch.Interfaces
{
    /// <summary>
    /// Responsible for rebuilding the Elasticsearch index
    /// from the SQL Server database.
    /// </summary>
    public interface IElasticBulkIndexer
    {
        /// <summary>
        /// Rebuilds the entire search index: schools, colleges, courses,
        /// and specializations.
        /// </summary>
        Task RebuildIndexAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Indexes all colleges.
        /// </summary>
        Task IndexCollegesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Indexes all schools.
        /// </summary>
        Task IndexSchoolsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Indexes all courses.
        /// </summary>
        Task IndexCoursesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Indexes all specializations.
        /// </summary>
        Task IndexSpecializationsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the existing Elasticsearch index.
        /// </summary>
        Task DeleteIndexAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates the Elasticsearch index and mappings.
        /// </summary>
        Task CreateIndexAsync(CancellationToken cancellationToken = default);
    }
}

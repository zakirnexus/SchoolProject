using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Search;

namespace SchoolProject.Services.Search
{
    public class SearchIndexBuilder
    {
        private readonly AppDbContext _context;

        public SearchIndexBuilder(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ElasticSearchDocument>> BuildDocumentsAsync(CancellationToken cancellationToken = default)
        {
            var docs = new List<ElasticSearchDocument>();

            var schools = await _context.Schools
                .Include(s => s.City)
                .Include(s => s.Locality)
                .Include(s => s.NsewcNav)
                .Include(s => s.Coed)
                .Include(s => s.Ownership)
                .Include(s => s.SchoolSyllabuses!)
                    .ThenInclude(ss => ss.Syllabus)
                .Where(s => s.IsActive)
                .ToListAsync(cancellationToken);

            docs.AddRange(schools.Select(s => new ElasticSearchDocument
            {
                Id = $"school-{s.InstituteId}",
                EntityType = SearchEntityType.School,
                EntityId = s.InstituteId,
                Title = s.InstituteName,
                Slug = s.InstituteSlug,
                Url = "/school/" + s.InstituteSlug,
                CityId = s.CityId,
                CityName = s.City?.CityName,
                CitySlug = s.City?.CitySlug,
                LocalityId = s.LocalityId,
                LocalityName = s.Locality?.LocalityName,
                LocalitySlug = s.Locality?.LocalitySlug,
                NsewcId = s.NsewcId,
                NsewcName = s.NsewcNav?.NsewcName,
                SyllabusIds = s.SchoolSyllabuses?.Select(x => x.SyllabusId).ToList() ?? new List<int>(),
                SyllabusNames = s.SchoolSyllabuses?.Select(x => x.Syllabus?.SyllabusName ?? "").Where(x => x != "").ToList() ?? new List<string>(),
                SyllabusSlugs = s.SchoolSyllabuses?.Select(x => x.Syllabus?.SyllabusSlug ?? "").Where(x => x != "").ToList() ?? new List<string>(),
                CoedId = s.CoedId,
                CoedName = s.Coed?.CoedName,
                OwnershipId = s.InstOwnershipId,
                OwnershipName = s.Ownership?.InstOwnershipType,
                Address = s.Address,
                Pincode = s.Pincode,
                Keywords = s.Keyword,
                MetaDescription = s.MetaDescription,
                Description = string.Join(" ", new[]
                {
                    s.AdmissionCriteria,
                    s.Extracurricular,
                    s.ClassesLevels,
                    s.FeesStructure
                }.Where(x => !string.IsNullOrWhiteSpace(x))),
                ListingRank = s.ListingRank,
                IsSponsored = s.IsSponsored,
                IsActive = s.IsActive,
                Suggest = new[]
                {
                    s.InstituteName ?? string.Empty,
                    s.InstituteSlug ?? string.Empty
                }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList()
            }));

            return docs;
        }
    }
}

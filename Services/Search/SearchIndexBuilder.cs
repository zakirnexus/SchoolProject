using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Colleges;
using SchoolProject.Models.Search;

namespace SchoolProject.Services.Search
{
    public class SearchIndexBuilder
    {
        private readonly AppDbContext _context;

        // Base relevance weight per entity type. Colleges/Schools are the
        // primary "listing" entities so they sit above pure taxonomy
        // entities like Course/Specialization, which normally surface as
        // secondary matches (e.g. "MBA" -> course + colleges offering it).
        private static readonly Dictionary<SearchEntityType, double> BaseEntityScore = new()
        {
            [SearchEntityType.College] = 60,
            [SearchEntityType.School] = 50,
            [SearchEntityType.Course] = 40,
            [SearchEntityType.Specialization] = 30
        };

        public SearchIndexBuilder(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ElasticSearchDocument>> BuildDocumentsAsync(CancellationToken cancellationToken = default)
        {
            var docs = new List<ElasticSearchDocument>();

            docs.AddRange(await BuildSchoolDocumentsAsync(cancellationToken));
            docs.AddRange(await BuildCollegeDocumentsAsync(cancellationToken));
            docs.AddRange(await BuildCourseDocumentsAsync(cancellationToken));
            docs.AddRange(await BuildSpecializationDocumentsAsync(cancellationToken));

            return docs;
        }

        // ------------------------------------------------------------------
        // SCHOOLS
        // ------------------------------------------------------------------
        private async Task<List<ElasticSearchDocument>> BuildSchoolDocumentsAsync(CancellationToken cancellationToken)
        {
            var schools = await _context.Schools
                .Include(s => s.City)
                    .ThenInclude(c => c!.State)
                .Include(s => s.Locality)
                .Include(s => s.NsewcNav)
                .Include(s => s.Coed)
                .Include(s => s.Ownership)
                .Include(s => s.SchoolSyllabuses!)
                    .ThenInclude(ss => ss.Syllabus)
                .Where(s => s.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return schools.Select(s =>
            {
                var syllabusIds = s.SchoolSyllabuses?.Select(x => x.SyllabusId).ToList() ?? new List<int>();
                var syllabusNames = s.SchoolSyllabuses?
                    .Select(x => x.Syllabus?.SyllabusName ?? "")
                    .Where(x => x != "")
                    .ToList() ?? new List<string>();
                var syllabusSlugs = s.SchoolSyllabuses?
                    .Select(x => x.Syllabus?.SyllabusSlug ?? "")
                    .Where(x => x != "")
                    .ToList() ?? new List<string>();

                var description = JoinNonEmpty(" ",
                    s.AdmissionCriteria,
                    s.Extracurricular,
                    s.ClassesLevels,
                    s.FeesStructure);

                var doc = new ElasticSearchDocument
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

                    SyllabusIds = syllabusIds,
                    SyllabusNames = syllabusNames,
                    SyllabusSlugs = syllabusSlugs,

                    CoedId = s.CoedId,
                    CoedName = s.Coed?.CoedName,

                    OwnershipId = s.InstOwnershipId,
                    OwnershipName = s.Ownership?.InstOwnershipType,

                    StateId = s.City?.StateId,
                    StateName = s.City?.State?.StateName,

                    Address = s.Address,
                    Pincode = s.Pincode,
                    Keywords = s.Keyword,
                    MetaDescription = s.MetaDescription,
                    Description = description,

                    ListingRank = s.ListingRank,
                    IsSponsored = s.IsSponsored,
                    IsFeatured = false, // Schools have no featured flag in the schema
                    IsActive = s.IsActive,

                    Logo = s.Logo,
                    CampusImage = s.Photos,
                    Accreditation = s.Accreditation,
                    EstablishedYear = ParseYear(s.Estd),
                    Website = null, // Schools have no website column
                    Phone = s.Telephone,

                    PlacementPercentage = null,
                    HostelAvailable = false,
                    WifiCampus = false,
                    ScholarshipAvailable = false,

                    Source = "School",
                    IndexedOn = DateTime.UtcNow
                };

                doc.SearchText = BuildSearchText(
                    doc.Title, doc.CityName, doc.LocalityName, doc.NsewcName,
                    doc.CoedName, doc.OwnershipName, doc.StateName,
                    doc.Keywords, doc.Address, description,
                    JoinNonEmpty(" ", syllabusNames.ToArray()));

                doc.Suggest = BuildSuggest(
                    doc.Title, doc.Slug, doc.CityName, doc.LocalityName);

                doc.BoostScore = ComputeBoost(
                    SearchEntityType.School, s.ListingRank, s.IsSponsored, isFeatured: false);

                return doc;
            }).ToList();
        }

        // ------------------------------------------------------------------
        // COLLEGES
        // ------------------------------------------------------------------
        private async Task<List<ElasticSearchDocument>> BuildCollegeDocumentsAsync(CancellationToken cancellationToken)
        {
            var colleges = await _context.Colleges
                .Include(c => c.City)
                .Include(c => c.Locality)
                .Include(c => c.State)
                .Include(c => c.Coed)
                .Include(c => c.Ownership)
                .Include(c => c.CollegeCourses!)
                    .ThenInclude(cc => cc.Course)
                .Include(c => c.CollegeCourses!)
                    .ThenInclude(cc => cc.SpecializationNav)
                .Where(c => c.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return colleges.Select(c =>
            {
                var activeCourses = c.CollegeCourses?
                    .Where(cc => cc.IsActive && cc.Course != null)
                    .ToList() ?? new List<CollegeCourse>();

                var courseIds = activeCourses
                    .Select(cc => cc.CourseId)
                    .Distinct()
                    .ToList();

                var courseNames = activeCourses
                    .Select(cc => cc.Course!.CourseName ?? "")
                    .Where(x => x != "")
                    .Distinct()
                    .ToList();

                var specializationNames = activeCourses
                    .Where(cc => cc.SpecializationNav != null)
                    .Select(cc => cc.SpecializationNav!.SpecializationName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();

                var doc = new ElasticSearchDocument
                {
                    Id = $"college-{c.InstituteId}",
                    EntityType = SearchEntityType.College,
                    EntityId = c.InstituteId,
                    Title = c.InstituteName,
                    Slug = c.InstituteSlug,
                    Url = "/college/" + c.InstituteSlug,

                    CityId = c.CityId,
                    CityName = c.City?.CityName,
                    CitySlug = c.City?.CitySlug,

                    LocalityId = c.LocalityId,
                    LocalityName = c.Locality?.LocalityName,
                    LocalitySlug = c.Locality?.LocalitySlug,

                    NsewcId = null,
                    NsewcName = null,

                    CoedId = c.CoedId,
                    CoedName = c.Coed?.CoedName,

                    OwnershipId = c.InstOwnershipId,
                    OwnershipName = c.Ownership?.InstOwnershipType,

                    StateId = c.StateId,
                    StateName = c.State?.StateName,

                    CourseIds = courseIds,
                    CourseNames = courseNames,
                    SpecializationNames = specializationNames,

                    Address = c.Address,
                    Pincode = c.Pincode,
                    Keywords = c.Keywords,
                    MetaDescription = c.MetaDescription,
                    Description = c.AboutInstitute,

                    ListingRank = c.ListingRank,
                    IsSponsored = c.IsSponsored,
                    IsFeatured = c.IsFeatured,
                    IsActive = c.IsActive,

                    Logo = c.Logo,
                    CampusImage = c.Photos,
                    Accreditation = JoinNonEmpty(", ", c.Accreditation, c.NaacGrade),
                    EstablishedYear = ParseYear(c.Estd),
                    Website = c.Website,
                    Phone = JoinNonEmpty(" / ", c.Mobile, c.Telephone),

                    PlacementPercentage = c.PlacementPercentage,
                    HostelAvailable = c.HostelAvailable,
                    WifiCampus = c.WifiCampus,
                    ScholarshipAvailable = c.ScholarshipAvailable,

                    Source = "College",
                    IndexedOn = DateTime.UtcNow
                };

                doc.SearchText = BuildSearchText(
                    doc.Title, doc.CityName, doc.LocalityName, doc.StateName,
                    doc.CoedName, doc.OwnershipName, doc.Keywords, doc.Address,
                    c.ApprovedBy, c.AffiliatedTo, c.TopRecruiters,
                    doc.Description,
                    JoinNonEmpty(" ", courseNames.ToArray()),
                    JoinNonEmpty(" ", specializationNames.ToArray()));

                doc.Suggest = BuildSuggest(
                    doc.Title, doc.Slug, doc.CityName, doc.LocalityName);

                doc.BoostScore = ComputeBoost(
                    SearchEntityType.College, c.ListingRank, c.IsSponsored, c.IsFeatured);

                return doc;
            }).ToList();
        }

        // ------------------------------------------------------------------
        // COURSES
        // ------------------------------------------------------------------
        private async Task<List<ElasticSearchDocument>> BuildCourseDocumentsAsync(CancellationToken cancellationToken)
        {
            var courses = await _context.Courses
                .Include(c => c.Level)
                .Include(c => c.Category)
                    .ThenInclude(cat => cat!.ParentCategory)
                .Where(c => c.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Number of colleges offering each course, used only to weight
            // BoostScore (more colleges offering a course -> more likely to
            // be a common, high-intent search term).
            var offeringCounts = await _context.CollegeCourses
                .Where(cc => cc.IsActive)
                .GroupBy(cc => cc.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseId, x => x.Count, cancellationToken);

            return courses.Select(c =>
            {
                var doc = new ElasticSearchDocument
                {
                    Id = $"course-{c.CourseId}",
                    EntityType = SearchEntityType.Course,
                    EntityId = c.CourseId,
                    Title = c.CourseName,
                    Slug = c.CourseSlug,
                    Url = "/courses/" + c.CourseSlug,

                    CourseIds = new List<int> { c.CourseId },
                    CourseNames = new List<string> { c.CourseName ?? "" }
                        .Where(x => x != "").ToList(),

                    Keywords = null,
                    MetaDescription = null,
                    Description = c.Description,

                    ListingRank = null,
                    IsSponsored = false,
                    IsFeatured = false,
                    IsActive = c.IsActive,

                    Source = "Course",
                    IndexedOn = DateTime.UtcNow
                };

                doc.SearchText = BuildSearchText(
                    doc.Title, c.CourseFullName, c.ShortName,
                    c.Level?.LevelName, c.Category?.CategoryName,
                    c.Category?.ParentCategory?.CategoryName,
                    c.Description);

                doc.Suggest = BuildSuggest(
                    doc.Title, c.CourseFullName, c.ShortName, doc.Slug);

                offeringCounts.TryGetValue(c.CourseId, out var offeringCount);

                doc.BoostScore = ComputeBoost(
                    SearchEntityType.Course,
                    listingRank: null,
                    isSponsored: false,
                    isFeatured: false,
                    popularityBoost: Math.Min(offeringCount, 100) * 0.2);

                return doc;
            }).ToList();
        }

        // ------------------------------------------------------------------
        // SPECIALIZATIONS
        // ------------------------------------------------------------------
        private async Task<List<ElasticSearchDocument>> BuildSpecializationDocumentsAsync(CancellationToken cancellationToken)
        {
            var specializations = await _context.Specializations
                .Include(s => s.Course)
                .Where(s => s.IsActive && !s.IsDeprecated)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var offeringCounts = await _context.CollegeCourses
                .Where(cc => cc.IsActive && cc.SpecializationId != null)
                .GroupBy(cc => cc.SpecializationId!.Value)
                .Select(g => new { SpecializationId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SpecializationId, x => x.Count, cancellationToken);

            return specializations.Select(s =>
            {
                var courseSlug = s.Course?.CourseSlug;
                var url = !string.IsNullOrWhiteSpace(courseSlug)
                    ? $"/courses/{courseSlug}/specializations/{s.SpecializationSlug}"
                    : "/specializations/" + s.SpecializationSlug;

                var doc = new ElasticSearchDocument
                {
                    Id = $"specialization-{s.SpecializationId}",
                    EntityType = SearchEntityType.Specialization,
                    EntityId = s.SpecializationId,
                    Title = s.SpecializationName,
                    Slug = s.SpecializationSlug,
                    Url = url,

                    CourseIds = s.CourseId > 0 ? new List<int> { s.CourseId } : new List<int>(),
                    CourseNames = new List<string> { s.Course?.CourseName ?? "" }
                        .Where(x => x != "").ToList(),
                    SpecializationNames = new List<string> { s.SpecializationName }
                        .Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),

                    Description = s.Course?.Description,

                    ListingRank = null,
                    IsSponsored = false,
                    IsFeatured = false,
                    IsActive = s.IsActive,

                    Source = "Specialization",
                    IndexedOn = DateTime.UtcNow
                };

                doc.SearchText = BuildSearchText(
                    doc.Title, s.SpecializationType, s.Course?.CourseName,
                    s.Course?.CourseFullName, s.Course?.ShortName);

                doc.Suggest = BuildSuggest(
                    doc.Title, doc.Slug, s.Course?.CourseName);

                offeringCounts.TryGetValue(s.SpecializationId, out var offeringCount);

                doc.BoostScore = ComputeBoost(
                    SearchEntityType.Specialization,
                    listingRank: null,
                    isSponsored: false,
                    isFeatured: false,
                    popularityBoost: Math.Min(offeringCount, 100) * 0.2);

                return doc;
            }).ToList();
        }

        // ------------------------------------------------------------------
        // HELPERS
        // ------------------------------------------------------------------

        /// <summary>
        /// Builds the free-text field used for full-text matching by joining
        /// every meaningful attribute of the entity into one lowercase-friendly
        /// blob. Null/empty/whitespace values are dropped.
        /// </summary>
        private static string BuildSearchText(params string?[] parts)
        {
            return JoinNonEmpty(" ", parts);
        }

        /// <summary>
        /// Builds the completion-suggester entries: the entity's own name/slug
        /// plus a couple of location qualifiers, deduplicated and trimmed.
        /// </summary>
        private static List<string> BuildSuggest(params string?[] parts)
        {
            return parts
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string JoinNonEmpty(string separator, params string?[] parts)
        {
            return string.Join(separator, parts.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        /// <summary>
        /// Extracts a 4-digit year (e.g. from "Est. 1998" or "1998") out of
        /// the free-text "estd" column. Returns null if none is found.
        /// </summary>
        private static int? ParseYear(string? estd)
        {
            if (string.IsNullOrWhiteSpace(estd))
                return null;

            var match = Regex.Match(estd, @"(19|20)\d{2}");
            return match.Success && int.TryParse(match.Value, out var year) ? year : null;
        }

        /// <summary>
        /// Composite relevance score combining a per-entity-type base weight,
        /// an inverse listing-rank bonus (lower rank number = better placement
        /// = higher score), flat bonuses for sponsored/featured listings, and
        /// an optional popularity bonus (e.g. number of colleges offering a
        /// course/specialization).
        /// </summary>
        private static double ComputeBoost(
            SearchEntityType entityType,
            int? listingRank,
            bool isSponsored,
            bool isFeatured,
            double popularityBoost = 0)
        {
            var score = BaseEntityScore.TryGetValue(entityType, out var baseScore) ? baseScore : 0;

            if (listingRank.HasValue && listingRank.Value > 0)
            {
                score += Math.Max(0, 100 - listingRank.Value);
            }

            if (isSponsored)
                score += 50;

            if (isFeatured)
                score += 30;

            score += popularityBoost;

            return score;
        }
    }
}
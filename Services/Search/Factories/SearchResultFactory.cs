using SchoolProject.Models;
using SchoolProject.Models.Colleges;
using SchoolProject.Models.Search;

namespace SchoolProject.Services.Search.Factories
{
    public static class SearchResultFactory
    {
        public static SearchResultViewModel BuildCollege(
            College college,
            List<string>? matchingCourses = null,
            int score = 100,
            string? matchReason = null)
        {
            int? establishedYear = null;

            if (!string.IsNullOrWhiteSpace(college.Estd) &&
                int.TryParse(college.Estd, out var year))
            {
                establishedYear = year;
            }

            var accreditation = BuildAccreditation(college);

            return new SearchResultViewModel
            {
                InstituteId = college.InstituteId,

                Title = college.InstituteName,

                Url = "/college/" + college.InstituteSlug,

                Type = "College",

                Score = score,

                Logo = college.Logo,

                CampusImage = college.Photos,

                Address = college.Address,

                EstablishedYear = establishedYear,

                Ownership = GetOwnership(college.InstOwnershipId),

                Accreditation = accreditation,

                Sponsored = college.IsSponsored,

                ListingRank = college.ListingRank ?? 0,

                Description = college.AboutInstitute,

                MatchReason = matchReason ?? "",

                MatchingCourses = matchingCourses ?? new List<string>(),

                Website = college.Website,

                Phone = college.Telephone
            };
        }

        private static string BuildAccreditation(College college)
        {
            var items = new List<string>();

            if (!string.IsNullOrWhiteSpace(college.Accreditation))
                items.Add(college.Accreditation);

            if (!string.IsNullOrWhiteSpace(college.NaacGrade))
                items.Add("NAAC " + college.NaacGrade);

            if (!string.IsNullOrWhiteSpace(college.ApprovedBy))
                items.Add(college.ApprovedBy);

            if (college.NbaAccredited == true)
                items.Add("NBA");

            return string.Join(" • ", items);
        }

        private static string? GetOwnership(int? ownershipId)
        {
            return ownershipId switch
            {
                1 => "Private",
                2 => "Government",
                _ => null
            };
        }
    }
}
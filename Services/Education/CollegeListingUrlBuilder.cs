using System.Text;

namespace SchoolProject.Services.Education
{
    public class CollegeListingUrlBuilder
    {
        public string Build(
            bool isCoaching,
            string courseSlug,
            string citySlug,
            int page,
            int? localityId = null,
            string? nsewc = null,
            int? coedId = null,
            int? ownershipId = null,
            string? feesRange = null)
        {
            var sb = new StringBuilder();

            sb.Append('/');

            sb.Append(courseSlug);

            sb.Append(isCoaching
                ? "-coaching-in-"
                : "-colleges-in-");

            sb.Append(citySlug);

            sb.Append("?page=");
            sb.Append(page);

            if (localityId.HasValue)
                sb.Append("&locality=").Append(localityId.Value);

            if (!string.IsNullOrWhiteSpace(nsewc))
                sb.Append("&nsewc=").Append(Uri.EscapeDataString(nsewc));

            if (coedId.HasValue)
                sb.Append("&coedId=").Append(coedId.Value);

            if (ownershipId.HasValue)
                sb.Append("&ownershipId=").Append(ownershipId.Value);

            if (!string.IsNullOrWhiteSpace(feesRange))
                sb.Append("&feesRange=").Append(Uri.EscapeDataString(feesRange));

            return sb.ToString();
        }
    }
}
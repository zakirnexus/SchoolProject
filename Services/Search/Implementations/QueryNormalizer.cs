using System.Text.RegularExpressions;
using SchoolProject.Services.Search.Interfaces;

namespace SchoolProject.Services.Search.Implementations
{
    public class QueryNormalizer : IQueryNormalizer
    {
        private static readonly string[] StopWords =
        {
            "of",
            "and",
            "the",
            "in",
            "&",
            "-",
            "for"
        };

        public List<string> Normalize(string query)
        {
            query = query.ToLower();

            query = Regex.Replace(query, @"[^\w\s]", " ");

            var words = query
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !StopWords.Contains(w))
                .Distinct()
                .ToList();

            return words;
        }
    }
}
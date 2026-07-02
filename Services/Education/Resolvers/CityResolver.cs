using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Lookups;

namespace SchoolProject.Services.Education.Resolvers
{
    public class CityResolver : ICityResolver
    {
        private readonly AppDbContext _context;

        public CityResolver(AppDbContext context)
        {
            _context = context;
        }

        public async Task<City?> ResolveAsync(string citySlug)
        {
            if (string.IsNullOrWhiteSpace(citySlug))
                return null;

            citySlug = citySlug.Trim('/');

            return await _context.Cities
                .FirstOrDefaultAsync(c =>
                    c.CitySlug != null &&
                    c.CitySlug.ToLower() == citySlug.ToLower());
        }
    }
}
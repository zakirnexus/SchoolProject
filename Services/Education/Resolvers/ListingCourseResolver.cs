using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Courses;
using SchoolProject.ViewModels.Education.Resolvers;

namespace SchoolProject.Services.Education.Resolvers
{
    public class ListingCourseResolver : IListingCourseResolver
    {
        private readonly AppDbContext _context;

        public ListingCourseResolver(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CourseResolutionResult> ResolveAsync(
            string slug,
            bool isCoaching)
        {
            var result = new CourseResolutionResult
            {
                RequestedSlug = slug,
                Success = false
            };

            int? specializationId = null;

            // ----------------------------------------------------
            // 1. Try CoursePage
            // ----------------------------------------------------

            var page = await _context.CoursePages
                .FirstOrDefaultAsync(p =>
                    p.PageSlug != null &&
                    p.PageSlug.ToLower() == slug.ToLower() &&
                    p.IsActive);

            if (page != null)
            {
                specializationId = page.SpecializationId;

                var course = await _context.Courses
                    .Include(c => c.Level)
                    .Include(c => c.Category)
                    .FirstOrDefaultAsync(c =>
                        c.CourseId == page.CourseId);

                if (course != null)
                {
                    if (isCoaching && !course.IsCoaching)
                    {
                        return result;
                    }

                    return new CourseResolutionResult
                    {
                        Success = true,
                        Course = course,
                        SpecializationId = specializationId,
                        RequestedSlug = slug,
                        DisplayName = course.CourseName
                    };
                }
            }

            // ----------------------------------------------------
// 2. Try Course Slug
// ----------------------------------------------------

var courseObj = await _context.Courses
    .Include(c => c.Level)
    .Include(c => c.Category)
    .FirstOrDefaultAsync(c =>
        c.CourseSlug != null &&
        c.CourseSlug.ToLower() == slug.ToLower());

if (courseObj != null)
{
    if (isCoaching && !courseObj.IsCoaching)
    {
        return result;
    }

    return new CourseResolutionResult
    {
        Success = true,
        Course = courseObj,
        RequestedSlug = slug,
        DisplayName = courseObj.CourseName
    };
}

// ----------------------------------------------------
// 3. Try Category
// ----------------------------------------------------

var category = await _context.CourseCategories
    .FirstOrDefaultAsync(c =>
        c.CategorySlug != null &&
        c.CategorySlug.ToLower() == slug.ToLower());

if (category != null)
{
    return new CourseResolutionResult
    {
        Success = true,
        IsCategory = true,
        Category = category,
        RequestedSlug = slug,
        DisplayName = category.CategoryName
    };
}

return result;
        }
    }
}
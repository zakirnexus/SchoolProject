using SchoolProject.Services.Search.Models;

using SchoolProject.Models.Courses;

namespace SchoolProject.Services.Search.Interfaces
{
    public interface ICourseAliasResolver
    {
        CourseAlias? ResolveAlias(string searchText);
        int? ResolveCourseId(string searchText);
    }
}
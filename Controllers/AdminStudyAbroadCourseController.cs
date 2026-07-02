using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.StudyAbroad;
using Microsoft.Data.SqlClient;

namespace SchoolProject.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    [Route("admin/study-abroad/courses")]
    public class AdminStudyAbroadCourseController : Controller
    {
            private readonly AppDbContext _context;

            public AdminStudyAbroadCourseController(AppDbContext context)
            {
                _context = context;
            }

            // LIST COURSES
            [HttpGet("")]
            public IActionResult Index()
            {
                var courses = _context.IntlCourses
                    .Include(c => c.Category)
                    .Include(c => c.Level)
                    .OrderBy(c => c.CourseName)
                    .ToList();

                return View(courses);
            }

            // CREATE COURSE (GET)
            [HttpGet("create")]
            public IActionResult Create()
            {
                LoadDropdowns();
                return View(new CourseIntl
                {
                    IsActive = true
                });
            }

            // CREATE COURSE (POST)
            [HttpPost("create")]
            public IActionResult Create(CourseIntl model)
            {
                if (!ModelState.IsValid)
                {
                    LoadDropdowns();
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    model.Slug = GenerateSlug(model.CourseName);
                }

                _context.IntlCourses.Add(model);
                _context.SaveChanges();

                TempData["Success"] = "Course created successfully.";
                return RedirectToAction(nameof(Index));
            }

            // EDIT COURSE (GET)
            [HttpGet("edit/{id:int}")]
            public IActionResult Edit(int id)
            {
                var course = _context.IntlCourses.Find(id);
                if (course == null)
                    return NotFound();

                LoadDropdowns();
                return View(course);
            }

            // EDIT COURSE (POST)
            [HttpPost("edit/{id:int}")]
            public IActionResult Edit(int id, CourseIntl model)
            {
                if (!ModelState.IsValid)
                {
                    LoadDropdowns();
                    return View(model);
                }

                var existing = _context.IntlCourses.Find(id);
                if (existing == null)
                    return NotFound();

                existing.CourseName = model.CourseName;
                existing.CourseCategoryId = model.CourseCategoryId;
                existing.LevelId = model.LevelId;
                existing.DegreeType = model.DegreeType;
                existing.Slug = string.IsNullOrWhiteSpace(model.Slug)
                    ? GenerateSlug(model.CourseName)
                    : model.Slug;
                existing.IsActive = model.IsActive;

                _context.SaveChanges();

                TempData["Success"] = "Course updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            // SOFT DELETE COURSE
            [HttpPost("delete/{id:int}")]
            public IActionResult Delete(int id)
            {
                var course = _context.IntlCourses.Find(id);
                if (course == null)
                    return NotFound();

                course.IsActive = false;
                _context.SaveChanges();

                TempData["Success"] = "Course deactivated successfully.";
                return RedirectToAction(nameof(Index));
            }

            // LIST INSTITUTE-COURSES FOR ONE INSTITUTE
            [HttpGet("institute/{instituteId:int}")]
            public IActionResult InstituteCourses(int instituteId)
            {
                var institute = _context.IntlInstitutes
                    .Include(i => i.InstituteCourses)
                        .ThenInclude(ic => ic.Course)
                            .ThenInclude(c => c.Level)
                    .FirstOrDefault(i => i.InstituteId == instituteId);

                if (institute == null)
                    return NotFound();

                LoadDropdowns(); // must set ViewBag.Courses
                return View(institute);
            }

         // ADD/EDIT INSTITUTE-COURSE
        [HttpPost("institute/{instituteId:int}/add-course")]
        public IActionResult AddInstituteCourse(int instituteId, InstituteCourse model)
        {
            

            // 1) Count BEFORE
            var before = _context.IntlInstituteCourses
                .Count(ic => ic.InstituteId == instituteId);
            System.Diagnostics.Debug.WriteLine($"BEFORE: rows for {instituteId} = {before}");

            model.InstituteId = instituteId;
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

    try
    {
        const string sql = @"
            INSERT INTO tb_intl_InstituteCourse
                (InstituteId, CourseId, DurationYears, DurationText,
                 CostMin, CostMax, CostPer, CostCurrency,
                 IsApproximateCost, RawHtml, CreatedAt, UpdatedAt)
            VALUES
                (@InstituteId, @CourseId, @DurationYears, @DurationText,
                 @CostMin, @CostMax, @CostPer, @CostCurrency,
                 @IsApproximateCost, @RawHtml, @CreatedAt, @UpdatedAt);";

        var rows = _context.Database.ExecuteSqlRaw(
            sql,
            new SqlParameter("@InstituteId", instituteId),
            new SqlParameter("@CourseId", model.CourseId),
            new SqlParameter("@DurationYears", (object?)model.DurationYears ?? DBNull.Value),
            new SqlParameter("@DurationText", (object?)model.DurationText ?? DBNull.Value),
            new SqlParameter("@CostMin", (object?)model.CostMin ?? DBNull.Value),
            new SqlParameter("@CostMax", (object?)model.CostMax ?? DBNull.Value),
            new SqlParameter("@CostPer", (object?)model.CostPer ?? DBNull.Value),
            new SqlParameter("@CostCurrency", (object?)model.CostCurrency ?? DBNull.Value),
            new SqlParameter("@IsApproximateCost", model.IsApproximateCost),
            new SqlParameter("@RawHtml", (object?)model.RawHtml ?? DBNull.Value),
            new SqlParameter("@CreatedAt", model.CreatedAt),
            new SqlParameter("@UpdatedAt", model.UpdatedAt)
        );

        System.Diagnostics.Debug.WriteLine($"Raw INSERT rows affected = {rows}");

        // 2) Count AFTER (same DbContext, same request)
        var after = _context.IntlInstituteCourses
            .Count(ic => ic.InstituteId == instituteId);
        System.Diagnostics.Debug.WriteLine($"AFTER: rows for {instituteId} = {after}");

        TempData["Success"] = "Institute course added successfully.";
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine("ERROR saving InstituteCourse:");
        System.Diagnostics.Debug.WriteLine(ex.ToString());
        TempData["Error"] = "Could not save institute course. See log for details.";
    }

    return RedirectToAction(nameof(InstituteCourses), new { instituteId });
}

            [HttpPost("institute-course/delete/{id:int}")]
            public IActionResult DeleteInstituteCourse(int id)
            {
                var ic = _context.IntlInstituteCourses.Find(id);
                if (ic == null)
                    return NotFound();

                var instituteId = ic.InstituteId;
                _context.IntlInstituteCourses.Remove(ic);
                _context.SaveChanges();

                TempData["Success"] = "Institute course removed.";
                return RedirectToAction(nameof(InstituteCourses), new { instituteId });
            }

            private void LoadDropdowns()
            {
                ViewBag.CourseCategories = _context.IntlCourseCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToList();

                ViewBag.CourseLevels = _context.IntlCourseLevels
                    .Where(l => l.IsActive == true)
                    .OrderBy(l => l.DisplayOrder)
                    .ToList();

                ViewBag.Institutes = _context.IntlInstitutes
                    .OrderBy(i => i.InstituteName)
                    .ToList();

                ViewBag.Courses = _context.IntlCourses
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CourseName)
                    .ToList();

                ViewBag.CourseNames = _context.IntlCourseNames
                    .OrderBy(n => n.CourseName)
                    .ToList();
            }

            private string GenerateSlug(string? text)
            {
                if (string.IsNullOrWhiteSpace(text))
                    return "";

                return text
                    .Trim()
                    .ToLower()
                    .Replace("&", "and")
                    .Replace(".", "")
                    .Replace(",", "")
                    .Replace("/", "-")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace(" ", "-")
                    .Replace("--", "-");
            }
    }
}
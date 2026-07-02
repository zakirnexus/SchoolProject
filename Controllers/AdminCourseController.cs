using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Courses;

namespace SchoolProject.Controllers
{
[Authorize(Roles = "Admin,Editor")]
public class AdminCourseController : Controller
{
private readonly AppDbContext _context;

    public AdminCourseController(AppDbContext context)
    {
        _context = context;
    }

    // =====================================================
    // LIST
    // =====================================================
    public IActionResult Index()
    {
        var courses = _context.Courses
            .Include(c => c.Level)
            .Include(c => c.Category)
            .OrderBy(c => c.CourseName)
            .ToList();

        return View(courses);
    }

    // =====================================================
    // CREATE (GET)
    // =====================================================
    public IActionResult Create()
    {
        LoadDropdowns();

        return View(new Course
        {
            IsActive = true
        });
    }

    // =====================================================
    // CREATE (POST)
    // =====================================================
    [HttpPost]
    public IActionResult Create(Course model)
    {
	Console.WriteLine($"CoreSubjectId={model.CoreSubjectId}");
	Console.WriteLine($"DegreeTypeId={model.DegreeTypeId}");
	Console.WriteLine($"CoreSpecializationId={model.CoreSpecializationId}");
        if (!ModelState.IsValid)
        {
            LoadDropdowns();
            return View(model);
        }

        // Auto slug generation
        if (string.IsNullOrWhiteSpace(model.CourseSlug))
        {
            model.CourseSlug = GenerateSlug(model.CourseName);
        }

        _context.Courses.Add(model);
        _context.SaveChanges();

        TempData["Success"] = "Course created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =====================================================
    // EDIT (GET)
    // =====================================================
    public IActionResult Edit(int id)
    {
        var course = _context.Courses.Find(id);

        if (course == null)
            return NotFound();

        LoadDropdowns(course.CourseId);

        return View(course);
    }

    // =====================================================
    // EDIT (POST)
    // =====================================================
    [HttpPost]
    public IActionResult Edit(Course model)
    {
        if (!ModelState.IsValid)
        {
            LoadDropdowns();
            return View(model);
        }

        var existing = _context.Courses.Find(model.CourseId);

        if (existing == null)
            return NotFound();

        existing.CourseName = model.CourseName;
        existing.ShortName = model.ShortName;
        existing.CourseSlug = model.CourseSlug;
        existing.CourseFullName = model.CourseFullName;

        existing.LevelId = model.LevelId;
        existing.CategoryId = model.CategoryId;

        existing.DurationYears = model.DurationYears;
        existing.Description = model.Description;

        existing.DegreeTypeId = model.DegreeTypeId;
        existing.CoreSubjectId = model.CoreSubjectId;
        existing.CoreSpecializationId = model.CoreSpecializationId;

        existing.IsActive = model.IsActive;

        if (string.IsNullOrWhiteSpace(existing.CourseSlug))
        {
            existing.CourseSlug = GenerateSlug(existing.CourseName);
        }

        _context.SaveChanges();

        TempData["Success"] = "Course updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =====================================================
    // DELETE (SOFT DELETE)
    // =====================================================
    public IActionResult Delete(int id)
    {
        var course = _context.Courses.Find(id);

        if (course != null)
        {
            course.IsActive = false;
            _context.SaveChanges();

            TempData["Success"] = "Course deactivated successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    // =====================================================
    // DROPDOWNS
    // =====================================================
    private void LoadDropdowns(int? courseId = null)
	{
		ViewBag.Levels = _context.CourseLevels
			.Where(x => x.IsActive)
			.OrderBy(x => x.DisplayOrder)
			.ToList();

		ViewBag.Categories = _context.CourseCategories
			.Where(x => x.IsActive)
			.OrderBy(x => x.CategoryName)
			.ToList();

		ViewBag.DegreeTypes = _context.DegreeTypes
			.Where(x => x.IsActive)
			.OrderBy(x => x.DegreeName)
			.ToList();

		ViewBag.Subjects = _context.Subjects
			.Where(x => x.IsActive)
			.OrderBy(x => x.SubjectName)
			.ToList();

		if (courseId.HasValue)
		{
			ViewBag.Specializations = _context.Specializations
				.Where(x =>
					x.IsActive &&
					x.CourseId == courseId.Value)
				.OrderBy(x => x.SpecializationName)
				.ToList();
		}
		else
		{
			ViewBag.Specializations = _context.Specializations
				.Where(x => x.IsActive)
				.OrderBy(x => x.SpecializationName)
				.ToList();
		}
	}

    // =====================================================
    // SLUG GENERATOR
    // =====================================================
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

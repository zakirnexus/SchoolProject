using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Courses;

namespace SchoolProject.Controllers
{
[Authorize(Roles = "Admin,Editor")]
public class AdminSpecializationController : Controller
{
private readonly AppDbContext _context;

    public AdminSpecializationController(AppDbContext context)
    {
        _context = context;
    }

    // =====================================================
    // LIST
    // =====================================================
    public IActionResult Index(int? courseId)
    {
        var query = _context.Specializations
            .Include(s => s.Course)
            .AsQueryable();

        if (courseId.HasValue)
        {
            query = query.Where(s => s.CourseId == courseId.Value);
        }

        ViewBag.Courses = _context.Courses
            .Where(c => c.IsActive)
            .OrderBy(c => c.CourseName)
            .ToList();

        var specializations = query
            .OrderBy(s => s.Course.CourseName)
            .ThenBy(s => s.SpecializationName)
            .ToList();

        return View(specializations);
    }

    // =====================================================
    // CREATE (GET)
    // =====================================================
    public IActionResult Create()
    {
        LoadDropdowns();

        return View(new Specialization
        {
            IsActive = true,
            IsDeprecated = false
        });
    }

    // =====================================================
    // CREATE (POST)
    // =====================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Specialization model)
    {
        if (!ModelState.IsValid)
        {
            LoadDropdowns();
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(model.SpecializationSlug))
        {
            model.SpecializationSlug =
                GenerateSlug(model.SpecializationName);
        }

        var exists = _context.Specializations.Any(x =>
        x.CourseId == model.CourseId &&
        x.SpecializationName.ToLower().Trim() ==
        model.SpecializationName.ToLower().Trim());

        if (exists)
        {
            ModelState.AddModelError("", "This specialization already exists for the selected course.");
            LoadDropdowns();
            return View(model);
        }
        _context.Specializations.Add(model);
        _context.SaveChanges();

        TempData["Success"] =
            "Specialization created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =====================================================
    // EDIT (GET)
    // =====================================================
    public IActionResult Edit(int id)
    {
        var specialization = _context.Specializations
            .FirstOrDefault(x => x.SpecializationId == id);

        if (specialization == null)
            return NotFound();

        LoadDropdowns();

        return View(specialization);
    }

    // =====================================================
    // EDIT (POST)
    // =====================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Specialization model)
    {
        if (!ModelState.IsValid)
        {
            LoadDropdowns();
            return View(model);
        }

        var existing = _context.Specializations
            .FirstOrDefault(x =>
                x.SpecializationId == model.SpecializationId);

        if (existing == null)
            return NotFound();

        var duplicate = _context.Specializations.Any(x =>
        x.SpecializationId != model.SpecializationId &&
        x.CourseId == model.CourseId &&
        x.SpecializationName.ToLower().Trim() ==
        model.SpecializationName.ToLower().Trim());

        if (duplicate)
        {
            ModelState.AddModelError("",
                "Another specialization with this name already exists for this course.");

            LoadDropdowns();
            return View(model);
        }
        
        existing.CourseId = model.CourseId;
        existing.SpecializationName = model.SpecializationName;
        existing.SpecializationSlug = model.SpecializationSlug;
        existing.IsActive = model.IsActive;
        existing.IsDeprecated = model.IsDeprecated;

        if (string.IsNullOrWhiteSpace(existing.SpecializationSlug))
        {
            existing.SpecializationSlug =
                GenerateSlug(existing.SpecializationName);
        }

        _context.SaveChanges();

        TempData["Success"] =
            "Specialization updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =====================================================
    // DEPRECATE
    // =====================================================
    public IActionResult Delete(int id)
    {
        var specialization = _context.Specializations
            .FirstOrDefault(x => x.SpecializationId == id);

        if (specialization != null)
        {
            specialization.IsDeprecated = true;
            specialization.IsActive = false;

            _context.SaveChanges();

            TempData["Success"] =
                "Specialization deprecated successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    // =====================================================
    // HELPERS
    // =====================================================
    private void LoadDropdowns()
    {
        ViewBag.Courses = _context.Courses
            .Where(c => c.IsActive)
            .OrderBy(c => c.CourseName)
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

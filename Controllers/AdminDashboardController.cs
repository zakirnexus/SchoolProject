using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;

namespace SchoolProject.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    public class AdminDashboardController : Controller
    {
        private readonly AppDbContext _context;

        public AdminDashboardController(AppDbContext context)
        {
            _context = context;
        }

       [HttpGet]
        [Route("AdminDashboard")]
        [Route("Admin")]
        public IActionResult Index()
        {
            // Stats (simple counts, read‑only)
            ViewBag.TotalSchools = _context.Schools
                .Where(s => s.IsActive)
                .AsNoTracking()
                .Count();

            ViewBag.TotalColleges = _context.Colleges
                .Where(c => c.IsActive)
                .AsNoTracking()
                .Count();

            var schoolEnquiryCount = _context.Enquiries
                .AsNoTracking()
                .Count();

            var collegeEnquiryCount = _context.CollegeEnquiries
                .AsNoTracking()
                .Count();

            ViewBag.TotalEnquiries = schoolEnquiryCount + collegeEnquiryCount;
            ViewBag.SchoolEnquiryCount = schoolEnquiryCount;
            ViewBag.CollegeEnquiryCount = collegeEnquiryCount;

            ViewBag.TotalBlogPosts = _context.BlogPosts
                .AsNoTracking()
                .Count();

            // Recent enquiries – small, read‑only lists
            ViewBag.RecentSchoolEnquiries = _context.Enquiries
                .Where(e => e.EntryDate != null)
                .OrderByDescending(e => e.EntryDate)
                .Take(5)
                .AsNoTracking()
                .ToList();

            ViewBag.RecentCollegeEnquiries = _context.CollegeEnquiries
                .Where(e => e.EntryDate != null)
                .Include(e => e.College)
                .OrderByDescending(e => e.EntryDate)
                .Take(5)
                .AsNoTracking()
                .ToList();

            return View();
        }
    }
}
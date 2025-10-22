using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using POE_Project.Data;
using POE_Project.Models;

namespace Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public HomeController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index() => View();

        // Lecturer
        [HttpGet]
        public IActionResult Lecturer() => View();

        [HttpPost]
        public IActionResult Lecturer(double hoursWorked, double hourlyRate, string notes, IFormFile? file)
        {
            if (hoursWorked <= 0 || hourlyRate <= 0)
            {
                ViewBag.Error = "Please enter valid values.";
                return View();
            }

            var claim = new Claim
            {
                HoursWorked = hoursWorked,
                HourlyRate = hourlyRate,
                Notes = notes,
                Status = "Pending"
            };

            // Save file if uploaded
            if (file != null && file.Length > 0)
            {
                var path = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                var filePath = Path.Combine(path, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                claim.Documents.Add(new Document
                {
                    FileName = file.FileName,
                    FilePath = "/uploads/" + file.FileName
                });
            }

            _context.Claims.Add(claim);
            _context.SaveChanges();

            ViewBag.Message = "Claim submitted successfully!";
            return View();
        }

        // Coordinator view
        public IActionResult Coordinator() => View(_context.Claims.Where(c => c.Status == "Pending").ToList());

        // Manager view
        public IActionResult Manager() => View(_context.Claims.ToList());

        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            var claim = _context.Claims.Find(id);
            if (claim == null) return NotFound();

            claim.Status = status;
            _context.SaveChanges();

            return RedirectToAction(status == "Approved" ? "Manager" : "Coordinator");
        }

        // Status tracking
        public IActionResult Status() => View(_context.Claims.ToList());
    }
}

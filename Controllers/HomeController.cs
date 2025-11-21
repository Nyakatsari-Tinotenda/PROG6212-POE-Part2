using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using POE_Project.Data;
using POE_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
        [Authorize(Roles = "Lecturer")]
        public IActionResult Lecturer() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Lecturer(Claim claim, List<IFormFile> files)
        {
            if (claim == null)
                return BadRequest();

            // Server-side validation
            if (claim.HoursWorked <= 0 || claim.HourlyRate <= 0)
            {
                ModelState.AddModelError("", "Hours Worked and Hourly Rate must be positive values.");
            }

            if (!ModelState.IsValid)
            {
                return View(claim);
            }

            // Set default status and calculate total
            claim.Status = "Pending";
            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;

            // Save claim first to get ClaimID
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            // File upload handling
            if (files != null && files.Count > 0)
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsRoot))
                    Directory.CreateDirectory(uploadsRoot);

                foreach (var file in files)
                {
                    if (file == null || file.Length == 0) continue;

                    // Server-side file validation
                    if (file.Length > 5 * 1024 * 1024) // 5MB limit
                    {
                        ModelState.AddModelError("", $"File {file.FileName} exceeds the 5MB size limit.");
                        continue;
                    }

                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
                    if (!allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("", $"File {file.FileName} has an invalid file type.");
                        continue;
                    }

                    // Generate unique filename for security
                    var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(uploadsRoot, uniqueFileName);

                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Save Document record
                        var document = new Document
                        {
                            FileName = file.FileName,
                            StoredFileName = uniqueFileName,
                            FilePath = "/uploads/" + uniqueFileName,
                            ContentType = file.ContentType,
                            SizeBytes = file.Length,
                            ClaimID = claim.ClaimID
                        };
                        _context.Documents.Add(document);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error uploading file {file.FileName}: {ex.Message}");
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["Message"] = "Claim submitted successfully!";
            return RedirectToAction(nameof(Lecturer));
        }

        // Coordinator view
        [Authorize(Roles = "Coordinator")]
        public IActionResult Coordinator() => View(_context.Claims.Where(c => c.Status == "Pending").ToList());

        // Manager view
        [Authorize(Roles = "Manager")]
        public IActionResult Manager() => View(_context.Claims.ToList());

        // HR view
        [Authorize(Roles = "HR")]
        public IActionResult HR() => View();

        [HttpPost]
        [Authorize(Roles = "Coordinator,Manager")]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordinator,Manager")]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.Status = "Approved";
            await _context.SaveChangesAsync();

            TempData["Message"] = "Claim approved successfully!";
            return RedirectToAction(nameof(Coordinator));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordinator,Manager")]
        public async Task<IActionResult> RejectClaim(int id, string? reason)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.Status = "Rejected";
            claim.Notes = (claim.Notes ?? "") + (string.IsNullOrWhiteSpace(reason) ? "" : $"\nRejection reason: {reason}");
            await _context.SaveChangesAsync();

            TempData["Message"] = "Claim rejected successfully!";
            return RedirectToAction(nameof(Coordinator));
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmcs.Models;
using cmcs.Data;
using System.Diagnostics;

namespace cmcs.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index");
            }

            if (email.ToLower().Contains("admin") || email.ToLower().Contains("coordinator") || email.ToLower().Contains("manager"))
            {
                TempData["UserRole"] = "Admin";
                TempData["UserEmail"] = email;
                return RedirectToAction("AdminDashboard");
            }
            else
            {
                TempData["UserRole"] = "Lecturer";
                TempData["UserEmail"] = email;
                return RedirectToAction("LecturerDashboard");
            }
        }

        public IActionResult Dashboard(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                email = "lecturer@iie.com";
            }

            if (email.ToLower().Contains("admin") || email.ToLower().Contains("coordinator") || email.ToLower().Contains("manager"))
            {
                var pendingClaims = _context.Claims
                    .Include(c => c.SupportingDocuments)
                    .Where(c => c.Status == "Submitted" || c.Status == "Under Review")
                    .OrderByDescending(c => c.SubmittedDate)
                    .ToList();

                return View("AdminDashboard", pendingClaims);
            }
            else
            {
                var userClaims = _context.Claims
                    .Include(c => c.SupportingDocuments)
                    .Where(c => c.LecturerEmail == email)
                    .OrderByDescending(c => c.SubmittedDate)
                    .ToList();

                return View("LecturerDashboard", userClaims);
            }
        }

        public IActionResult AdminDashboard()
        {
            if (TempData["UserRole"] as string != "Admin")
            {
                return RedirectToAction("Index");
            }

            var pendingClaims = _context.Claims
                .Include(c => c.SupportingDocuments)
                .Where(c => c.Status == "Submitted" || c.Status == "Under Review")
                .OrderByDescending(c => c.SubmittedDate)
                .ToList();

            ViewBag.UserEmail = TempData["UserEmail"];
            ViewBag.UserRole = "Administrator";

            return View("AdminDashboard", pendingClaims);
        }

        public IActionResult LecturerDashboard()
        {
            var userEmail = TempData["UserEmail"] as string ?? "lecturer@iie.com";

            var userClaims = _context.Claims
                .Include(c => c.SupportingDocuments)
                .Where(c => c.LecturerEmail == userEmail)
                .OrderByDescending(c => c.SubmittedDate)
                .ToList();

            ViewBag.UserEmail = userEmail;
            ViewBag.UserRole = "Lecturer";

            return View("LecturerDashboard", userClaims);
        }

        [HttpGet]
        public IActionResult SubmitClaim()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(Claim claim, List<IFormFile> supportingDocs)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    claim.LecturerName = "Demo Lecturer";
                    claim.LecturerEmail = "lecturer@iie.com";
                    claim.Status = "Submitted";
                    claim.SubmittedDate = DateTime.Now;

                    if (claim.ClaimMonth.Year < 2000)
                    {
                        claim.ClaimMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    }

                    _context.Claims.Add(claim);
                    await _context.SaveChangesAsync();

                    if (supportingDocs != null && supportingDocs.Count > 0)
                    {
                        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");

                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        foreach (var file in supportingDocs)
                        {
                            if (file.Length > 0 && file.Length < 10 * 1024 * 1024)
                            {
                                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif" };
                                var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();

                                if (!allowedExtensions.Contains(fileExtension))
                                {
                                    continue;
                                }

                                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                }

                                var document = new SupportingDocument
                                {
                                    FileName = file.FileName,
                                    FilePath = uniqueFileName,
                                    FileSize = file.Length,
                                    UploadDate = DateTime.Now,
                                    ClaimId = claim.ClaimId
                                };

                                _context.SupportingDocuments.Add(document);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Claim submitted successfully!";
                    return RedirectToAction("LecturerDashboard");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error submitting claim");
                    ModelState.AddModelError("", "An error occurred while submitting your claim. Please try again.");
                }
            }

            return View(claim);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = "Approved";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Claim #{id} has been approved.";
            }
            return RedirectToAction("AdminDashboard");
        }

        [HttpPost]
        public async Task<IActionResult> RejectClaim(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = "Rejected";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Claim #{id} has been rejected.";
            }
            return RedirectToAction("AdminDashboard");
        }

        public IActionResult TrackClaim(int id)
        {
            var claim = _context.Claims
                .Include(c => c.SupportingDocuments)
                .FirstOrDefault(c => c.ClaimId == id);

            if (claim == null)
            {
                return NotFound();
            }

            var statusHistory = new List<string>
            {
                $"{claim.SubmittedDate:yyyy-MM-dd HH:mm:ss} - Claim Submitted by Lecturer."
            };

            if (claim.Status == "Under Review")
            {
                statusHistory.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Claim is under review by Programme Coordinator.");
            }
            else if (claim.Status == "Approved")
            {
                statusHistory.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Claim has been approved.");
            }
            else if (claim.Status == "Rejected")
            {
                statusHistory.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Claim has been rejected.");
            }

            ViewBag.StatusHistory = statusHistory;
            return View(claim);
        }

        public IActionResult ReviewClaim(int id)
        {
            var claim = _context.Claims
                .Include(c => c.SupportingDocuments)
                .FirstOrDefault(c => c.ClaimId == id);

            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }

        public async Task<IActionResult> DownloadDocument(int id)
        {
            var document = await _context.SupportingDocuments.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", document.FilePath);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(filePath), document.FileName);
        }

        private string GetContentType(string path)
        {
            var dotIndex = path.LastIndexOf('.');
            if (dotIndex == -1) return "application/octet-stream";

            var extension = path.Substring(dotIndex).ToLowerInvariant();

            var types = new Dictionary<string, string>
            {
                { ".pdf", "application/pdf" },
                { ".doc", "application/msword" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" },
                { ".gif", "image/gif" }
            };

            return types.ContainsKey(extension) ? types[extension] : "application/octet-stream";
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
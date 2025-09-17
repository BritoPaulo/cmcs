using cmcs.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace cmcs.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // This action handles the login form and decides which dashboard to show
        public IActionResult Dashboard(string email)
        {
            // Simple logic for the non-functional prototype
            if (email != null && email.ToLower().Contains("admin"))
            {
                return View("AdminDashboard"); // Show the admin/coordinator view
            }
            else
            {
                return View("LecturerDashboard"); // Show the lecturer view by default
            }
        }

        // Action to show the form for submitting a new claim
        public IActionResult SubmitClaim()
        {
            return View();
        }

        // ... (Other code for Privacy, Error, etc.)
    

    public IActionResult TrackClaim()
        {
            // For the prototype, we will create a sample claim to display.
            // In a real application, this would come from a database based on the logged-in user.
            var sampleClaim = new Claim
            {
                ClaimId = 1001,
                ClaimMonth = new DateTime(2025, 03, 01),
                TotalHours = 40.5m,
                TotalAmount = 12150.00m,
                Status = "Under Review",
                SubmittedDate = new DateTime(2025, 03, 05)
            };

            // We can also create a simple list of status history for tracking.
            // This is static data for the prototype.
            ViewBag.StatusHistory = new List<string>
    {
        "2025-03-05 14:30:02 - Claim Submitted by Lecturer.",
        "2025-03-06 09:15:47 - Claim received by System. Awaiting review.",
        "2025-03-06 10:01:22 - Claim is under review by Programme Coordinator."
    };

            return View(sampleClaim); // Pass the sample claim to the view
        }
    }
}
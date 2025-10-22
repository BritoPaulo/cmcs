using cmcs.Controllers;
using cmcs.Data;
using cmcs.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace cmcs.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly ApplicationDbContext _context;

        public HomeControllerTests()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(m => m.WebRootPath).Returns("wwwroot");

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            var controller = new HomeController(_mockLogger.Object, _context, _mockEnvironment.Object);

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);


            //Install - Package Microsoft.EntityFrameworkCore.InMemory - Version 9.0.0
//Install - Package Microsoft.NET.Test.Sdk - Version 17.11.0
//Install - Package xunit - Version 2.8.1
//Install - Package xunit.runner.visualstudio - Version 2.8.1
        }

        [Fact]
        public void SubmitClaim_Get_ReturnsViewResult()
        {
            // Arrange
            var controller = new HomeController(_mockLogger.Object, _context, _mockEnvironment.Object);

            // Act
            var result = controller.SubmitClaim();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task SubmitClaim_Post_ValidModel_RedirectsToDashboard()
        {
            // Arrange
            var controller = new HomeController(_mockLogger.Object, _context, _mockEnvironment.Object);
            var claim = new Claim
            {
                ClaimMonth = new DateTime(2025, 1, 1),
                TotalHours = 40m,
                RatePerHour = 300m,
                Notes = "Test claim"
            };

            // Act
            var result = await controller.SubmitClaim(claim, null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("LecturerDashboard", redirectResult.ActionName);
        }

        [Fact]
        public async Task ApproveClaim_ValidId_UpdatesStatus()
        {
            // Arrange
            var claim = new Claim
            {
                ClaimMonth = new DateTime(2025, 1, 1),
                TotalHours = 40m,
                RatePerHour = 300m,
                Status = "Submitted"
            };
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            var controller = new HomeController(_mockLogger.Object, _context, _mockEnvironment.Object);

            // Act
            var result = await controller.ApproveClaim(claim.ClaimId);

            // Assert
            var updatedClaim = _context.Claims.Find(claim.ClaimId);
            Assert.Equal("Approved", updatedClaim.Status);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task RejectClaim_ValidId_UpdatesStatus()
        {
            // Arrange
            var claim = new Claim
            {
                ClaimMonth = new DateTime(2025, 1, 1),
                TotalHours = 40m,
                RatePerHour = 300m,
                Status = "Submitted"
            };
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            var controller = new HomeController(_mockLogger.Object, _context, _mockEnvironment.Object);

            // Act
            var result = await controller.RejectClaim(claim.ClaimId);

            // Assert
            var updatedClaim = _context.Claims.Find(claim.ClaimId);
            Assert.Equal("Rejected", updatedClaim.Status);
            Assert.IsType<RedirectToActionResult>(result);
        }
        //Dispose
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
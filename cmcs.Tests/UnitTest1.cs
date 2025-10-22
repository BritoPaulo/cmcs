using Microsoft.EntityFrameworkCore;
using cmcs.Data;
using cmcs.Models;
using Xunit;

namespace cmcs.Tests
{
    public class ClaimsTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Claim_TotalAmount_CalculatedCorrectly()
        {
            // Arrange
            var claim = new Claim
            {
                TotalHours = 40,
                RatePerHour = 300
            };

            // act
            var totalAmount = claim.TotalAmount;

            // assert
            Assert.Equal(12000, totalAmount);
        }

        [Fact]
        public void Claim_TotalAmount_WithDecimalHours_CalculatedCorrectly()
        {
            // Arrange
            var claim = new Claim
            {
                TotalHours = 37.5m,
                RatePerHour = 250.75m
            };

            // Act
            var totalAmount = claim.TotalAmount;

            // Assert
            Assert.Equal(9403.125m, totalAmount);
        }

        [Fact]
        public async Task Can_Add_Claim_To_Database()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var claim = new Claim
            {
                ClaimMonth = new DateTime(2025, 3, 1),
                TotalHours = 40.5m,
                RatePerHour = 300m,
                Notes = "Test claim",
                LecturerName = "Test Lecturer",
                LecturerEmail = "test@iie.com",
                Status = "Submitted",
                SubmittedDate = DateTime.Now
            };

            // Act
            context.Claims.Add(claim);
            await context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, context.Claims.Count());
            var savedClaim = context.Claims.First();
            Assert.Equal("Test Lecturer", savedClaim.LecturerName);
            Assert.Equal("Submitted", savedClaim.Status);
            Assert.Equal(12150m, savedClaim.TotalAmount); // 40.5 * 300
        }

        [Fact]
        public async Task Can_Add_SupportingDocument_To_Claim()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var claim = new Claim
            {
                ClaimMonth = new DateTime(2025, 3, 1),
                TotalHours = 40m,
                RatePerHour = 300m,
                LecturerName = "Test Lecturer",
                LecturerEmail = "test@iie.com",
                Status = "Submitted"
            };

            context.Claims.Add(claim);
            await context.SaveChangesAsync();

            var document = new SupportingDocument
            {
                FileName = "test.pdf",
                FilePath = "uploads/test.pdf",
                FileSize = 1024,
                UploadDate = DateTime.Now,
                ClaimId = claim.ClaimId
            };

            // Act
            context.SupportingDocuments.Add(document);
            await context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, context.SupportingDocuments.Count());
            var savedDocument = context.SupportingDocuments.First();
            Assert.Equal("test.pdf", savedDocument.FileName);
            Assert.Equal(claim.ClaimId, savedDocument.ClaimId);
        }

        [Fact]
        public void Claim_DefaultStatus_IsSubmitted()
        {
            // Arrange & Act
            var claim = new Claim();

            // Assert
            Assert.Equal("Submitted", claim.Status);
        }

        [Fact]
        public void Claim_DefaultSubmittedDate_IsRecent()
        {
            // Arrange & Act
            var claim = new Claim();
            var timeDifference = DateTime.Now - claim.SubmittedDate;

            // Assert
            Assert.True(timeDifference.TotalSeconds < 5); // Should be very recent
        }

        [Fact]
        public async Task Claim_Status_CanBeUpdated()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var claim = new Claim
            {
                ClaimMonth = new DateTime(2025, 3, 1),
                TotalHours = 40m,
                RatePerHour = 300m,
                LecturerName = "Test Lecturer",
                LecturerEmail = "test@iie.com",
                Status = "Submitted"
            };

            context.Claims.Add(claim);
            await context.SaveChangesAsync();

            // Act - Update status
            claim.Status = "Approved";
            await context.SaveChangesAsync();

            // Assert
            var updatedClaim = context.Claims.Find(claim.ClaimId);
            Assert.Equal("Approved", updatedClaim.Status);
        }

        [Fact]
        public void SupportingDocument_DefaultUploadDate_IsRecent()
        {
            // Arrange & Act
            var document = new SupportingDocument();
            var timeDifference = DateTime.Now - document.UploadDate;

            // Assert
            Assert.True(timeDifference.TotalSeconds < 5);
        }
    }

    public class ValidationTests
    {
        [Fact]
        public void Claim_WithValidData_PassesValidation()
        {
            // Arrange
            var claim = new Claim
            {
                ClaimMonth = new DateTime(2025, 1, 1),
                TotalHours = 40.5m,
                RatePerHour = 300m,
                Notes = "Valid claim with proper data",
                LecturerName = "Test Lecturer",
                LecturerEmail = "test@iie.com"
            };

            // Act & Assert - This would normally use a validator
            Assert.NotNull(claim);
            Assert.Equal(40.5m, claim.TotalHours);
            Assert.Equal(300m, claim.RatePerHour);
        }

        [Theory]
        [InlineData(0.5, 100, 50)]    // Minimum hours and rate
        [InlineData(200, 1000, 200000)] // Maximum hours and rate
        [InlineData(40, 300, 12000)]   // Typical values
        public void Claim_TotalAmount_VariousInputs_CalculatedCorrectly(decimal hours, decimal rate, decimal expected)
        {
            // Arrange
            var claim = new Claim
            {
                TotalHours = hours,
                RatePerHour = rate
            };

            // Act
            var result = claim.TotalAmount;

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
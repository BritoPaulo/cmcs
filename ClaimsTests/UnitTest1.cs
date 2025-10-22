using Microsoft.EntityFrameworkCore;
using cmcs.Data;
using cmcs.Models;

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

            // Act
            var totalAmount = claim.TotalAmount;

            // Assert
            Assert.Equal(12000, totalAmount);
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
                Status = "Submitted"
            };

            // Act
            context.Claims.Add(claim);
            await context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, context.Claims.Count());
            Assert.Equal("Test Lecturer", context.Claims.First().LecturerName);
        }
    }
}
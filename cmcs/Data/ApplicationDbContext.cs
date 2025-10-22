using Microsoft.EntityFrameworkCore;
using cmcs.Models;

namespace cmcs.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Claim>()
                .HasMany(c => c.SupportingDocuments)
                .WithOne(sd => sd.Claim)
                .HasForeignKey(sd => sd.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
using BuildMaX.Web.Models.Domain;
using BuildMaX.Web.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BuildMaX.Web.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Variant> Variants => Set<Variant>();
        public DbSet<AnalysisRequest> AnalysisRequests => Set<AnalysisRequest>();
        public DbSet<LegalDocument> LegalDocuments => Set<LegalDocument>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Variant (1) -> AnalysisRequest (N)
            builder.Entity<AnalysisRequest>()
                .HasOne(a => a.ApplicationUser)
                .WithMany()
                .HasForeignKey(a => a.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Opcjonalnie: Variant (1) -> LegalDocument (N)
            builder.Entity<LegalDocument>()
                .HasOne(d => d.Variant)
                .WithMany()
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

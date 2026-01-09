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

            // ApplicationUser (1) -> AnalysisRequest (N) [CASCADE]
            builder.Entity<AnalysisRequest>()
                .HasOne(a => a.ApplicationUser)
                .WithMany(u => u.AnalysisRequests)
                .HasForeignKey(a => a.ApplicationUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Variant (1) -> AnalysisRequest (N) [RESTRICT]
            // Usunięcie wariantu blokowane, jeśli istnieją zlecenia
            builder.Entity<AnalysisRequest>()
                .HasOne(a => a.Variant)
                .WithMany(v => v.AnalysisRequests)
                .HasForeignKey(a => a.VariantId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Variant (1) -> LegalDocument (N) [SET NULL]
            // Dokumenty prawne pozostają, a VariantId staje się NULL po usunięciu wariantu
            builder.Entity<LegalDocument>()
                .HasOne(d => d.Variant)
                .WithMany() // opcjonalnie: .WithMany(v => v.LegalDocuments) jeśli dodasz kolekcję
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indeksy (praktyczne pod Twoje LINQ i listy)
            builder.Entity<AnalysisRequest>()
                .HasIndex(a => new { a.ApplicationUserId, a.CreatedAt });

            builder.Entity<AnalysisRequest>()
                .HasIndex(a => new { a.VariantId, a.Status });

            builder.Entity<AnalysisRequest>()
                .HasIndex(a => a.BuiltUpPercent);

            builder.Entity<LegalDocument>()
                .HasIndex(d => new { d.VariantId, d.Category });
        }
    }
}

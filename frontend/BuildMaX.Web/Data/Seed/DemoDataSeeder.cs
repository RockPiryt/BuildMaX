// (warianty, dokumenty prawne)
using BuildMaX.Web.Models.Domain;
using Microsoft.EntityFrameworkCore;
using BuildMaX.Web.Models.Domain.Enums;

namespace BuildMaX.Web.Data.Seed
{
    public static class DemoDataSeeder
    {
        public static async Task SeedVariantsAsync(IServiceProvider sp)
        {
            var db = sp.GetRequiredService<AppDbContext>();

            if (await db.Variants.AsNoTracking().AnyAsync())
                return;

            var variants = new List<Variant>
            {
                new Variant
                {
                    Name = "Wariant 1",
                    Price = 499m,
                    Description = "Obliczenie maksymalnej powierzchni zabudowy i wymaganej powierzchni zielonej + zestaw dokumentów prawnych PDF.",
                    IncludesPdf = true,
                    IncludesPercentDetails = false,
                    IncludesSitePlan = false
                },
                new Variant
                {
                    Name = "Wariant 2",
                    Price = 899m,
                    Description = "Wariant 1 + szczegółowe dane procentowe (zabudowa, zieleń, utwardzenia, parkingi, obszary problemowe).",
                    IncludesPdf = true,
                    IncludesPercentDetails = true,
                    IncludesSitePlan = false
                },
                new Variant
                {
                    Name = "Wariant 3",
                    Price = 1499m,
                    Description = "Wariant 2 + plan zagospodarowania (magazyn, zabudowa pomocnicza, zieleń, utwardzenia).",
                    IncludesPdf = true,
                    IncludesPercentDetails = true,
                    IncludesSitePlan = true
                }
            };

            db.Variants.AddRange(variants);
            await db.SaveChangesAsync();
        }

        public static async Task SeedLegalDocumentsAsync(IServiceProvider sp)
        {
            var db = sp.GetRequiredService<AppDbContext>();

            if (await db.LegalDocuments.AsNoTracking().AnyAsync())
                return;

            // Pobierz warianty (mogą nie istnieć, jeśli ktoś pominął SeedVariants)
            var v1 = await db.Variants.AsNoTracking().FirstOrDefaultAsync(v => v.Name == "Wariant 1");
            var v2 = await db.Variants.AsNoTracking().FirstOrDefaultAsync(v => v.Name == "Wariant 2");
            var v3 = await db.Variants.AsNoTracking().FirstOrDefaultAsync(v => v.Name == "Wariant 3");

            var docs = new List<LegalDocument>
            {
                // ogólne (bez wariantu)
                new LegalDocument
                {
                    Title = "Prawo budowlane (ustawa)",
                    Url = "https://isap.sejm.gov.pl/",
                    Category = "Prawo"
                },
                new LegalDocument
                {
                    Title = "Warunki techniczne – rozporządzenie (WT)",
                    Url = "https://isap.sejm.gov.pl/",
                    Category = "Prawo"
                },

                // pod warianty (przykładowo)
                new LegalDocument
                {
                    Title = "MPZP – wyszukiwarka i uchwały (przykład źródła)",
                    Url = "https://www.gdansk.pl/",
                    Category = "MPZP",
                    VariantId = v1?.VariantId
                },
                new LegalDocument
                {
                    Title = "Wytyczne powierzchni biologicznie czynnej (przykład)",
                    Url = "https://www.gov.pl/",
                    Category = "Wskaźniki",
                    VariantId = v1?.VariantId
                },
                new LegalDocument
                {
                    Title = "Standard wyliczeń procentowych – opis metodyki",
                    Url = "https://www.buildmax.com/",
                    Category = "Metodyka",
                    VariantId = v2?.VariantId
                },
                new LegalDocument
                {
                    Title = "Plan zagospodarowania – wymagania minimalne (checklista)",
                    Url = "https://www.buildmax.com/",
                    Category = "Plan",
                    VariantId = v3?.VariantId
                }
            };

            db.LegalDocuments.AddRange(docs);
            await db.SaveChangesAsync();
        }

        // Demo zlecenia
        public static async Task SeedAnalysisRequestsAsync(IServiceProvider sp)
        {
            var db = sp.GetRequiredService<AppDbContext>();
            if (await db.AnalysisRequests.AnyAsync()) return;

            var client = await db.Users.FirstOrDefaultAsync(u => u.Email == "client@buildmax.local");
            var variant = await db.Variants.FirstAsync();

            if (client == null || variant == null) return;

            db.AnalysisRequests.AddRange(
                new AnalysisRequest
                {
                    ApplicationUserId = client.Id,
                    VariantId = variant.VariantId,
                    Address = "ul. Przemysłowa 1, Gdańsk",
                    PlotAreaM2 = 5000,
                    ModuleAreaM2 = 2500,
                    BuiltUpPercent = 50,
                    GreenAreaM2 = 1500,
                    HardenedAreaM2 = 1000,
                    TruckParkingSpots = 10,
                    CarParkingSpots = 40,
                    Status = AnalysisStatus.Completed,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new AnalysisRequest
                {
                    ApplicationUserId = client.Id,
                    VariantId = variant.VariantId,
                    Address = "ul. Logistyczna 10, Gdańsk",
                    PlotAreaM2 = 3000,
                    ModuleAreaM2 = 1200,
                    BuiltUpPercent = 40,
                    GreenAreaM2 = 900,
                    HardenedAreaM2 = 900,
                    TruckParkingSpots = 6,
                    CarParkingSpots = 20,
                    Status = AnalysisStatus.Processing,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
using System.Security.Claims;
using BuildMaX.Web.Data;
using BuildMaX.Web.Models.Domain;
using BuildMaX.Web.Models.Domain.Enums;
using BuildMaX.Web.Models.ViewModels.Analysis;
using BuildMaX.Web.Services.Analysis;
using BuildMaX.Web.Services.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildMaX.Web.Controllers
{
    [Authorize]
    public class AnalysisRequestsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IAnalysisCalculator _calc;
        private readonly IPdfReportService _pdf;

        public AnalysisRequestsController(AppDbContext db, IAnalysisCalculator calc, IPdfReportService pdf)
        {
            _db = db;
            _calc = calc;
            _pdf = pdf;
        }

        // LISTA: User widzi swoje; Admin/Analyst widzą wszystko
        public async Task<IActionResult> Index(AnalysisStatus? status, string? q, string? city)
        {
            var isAdminOrAnalyst = User.IsInRole("Admin") || User.IsInRole("Analyst");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _db.AnalysisRequests
                .AsNoTracking()
                .Include(a => a.Variant)
                .Include(a => a.ApplicationUser)
                .AsQueryable();

            if (!isAdminOrAnalyst)
                query = query.Where(a => a.ApplicationUserId == userId);

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(a => a.Address.Contains(q));

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(a => a.City != null && a.City.Contains(city));

            var data = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewData["SelectedStatus"] = status;
            ViewData["Query"] = q;
            ViewData["City"] = city;

            return View(data);
        }

        // SZCZEGÓŁY: User tylko swoje
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();

            var ar = await _db.AnalysisRequests
                .AsNoTracking()
                .Include(a => a.Variant)
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(a => a.AnalysisRequestId == id.Value);

            if (ar is null) return NotFound();
            if (!CanAccess(ar)) return Forbid();

            return View(ar);
        }

        // CREATE: wariant jest już kliknięty (Pricing -> Create?variantId=...)
        [Authorize(Roles = "Client,Admin")]
        [HttpGet]
        public async Task<IActionResult> Create(int variantId)
        {
            var variant = await _db.Variants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant is null) return NotFound();

            ViewBag.VariantName = variant.Name;

            var vm = new CreateAnalysisRequestViewModel
            {
                VariantId = variantId,
                AddressKind = AddressKind.Address
            };

            return View(vm);
        }

        // CREATE POST: user podaje wymiary + rozbity adres
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client,Admin")]
        public async Task<IActionResult> Create([Bind(
            "VariantId,AddressKind,Country,City,PostalCode,Street,StreetNumber,PlotNumber,CadastralArea,Commune," +
            "PlotWidthM,PlotLengthM,ModuleWidthM,ModuleLengthM"
        )] AnalysisRequest ar)
        {
            ar.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var variant = await _db.Variants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VariantId == ar.VariantId);

            if (variant is null)
                ModelState.AddModelError(nameof(ar.VariantId), "Wybrany wariant nie istnieje.");

            // Walidacja zależna od trybu
            if (ar.AddressKind == AddressKind.Address)
            {
                if (string.IsNullOrWhiteSpace(ar.City))
                    ModelState.AddModelError(nameof(ar.City), "Miasto jest wymagane.");
            }
            else // Plot
            {
                if (string.IsNullOrWhiteSpace(ar.PlotNumber))
                    ModelState.AddModelError(nameof(ar.PlotNumber), "Numer działki jest wymagany.");
            }

            // Składamy Address (bo masz [Required] w encji)
            ar.Address = BuildDisplayAddress(ar);

            // Usuwamy walidację pól systemowych, których nie ma w formularzu
            ModelState.Remove(nameof(AnalysisRequest.ApplicationUserId));
            ModelState.Remove(nameof(AnalysisRequest.ApplicationUser));
            ModelState.Remove(nameof(AnalysisRequest.Variant));

            if (!ModelState.IsValid)
            {
                ar.Variant = variant!;
                return View(ar);
            }

            ar.Status = AnalysisStatus.New;
            ar.CreatedAt = DateTime.UtcNow;

            _calc.ComputeAndApply(ar, new AnalysisAssumptions
            {
                GreenPercent = 0.25m,
                GeoGridsHardenedReduction = 0.0m
            });

            _db.AnalysisRequests.Add(ar);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = ar.AnalysisRequestId });
        }


        // EDIT GET: Admin/Analyst
        [Authorize(Roles = "Admin,Analyst")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();

            var ar = await _db.AnalysisRequests
                .Include(a => a.Variant)
                .FirstOrDefaultAsync(a => a.AnalysisRequestId == id.Value);

            if (ar is null) return NotFound();

            return View(ar);
        }

        // EDIT POST:
        // - Analyst: tylko status + pola wynikowe/ryzyka
        // - Admin: może zmienić wejście (wymiary) i wtedy robimy auto-przeliczenie
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Analyst")]
        public async Task<IActionResult> Edit(int id, AnalysisRequest input)
        {
            if (id != input.AnalysisRequestId) return NotFound();

            var ar = await _db.AnalysisRequests
                .Include(a => a.Variant)
                .FirstOrDefaultAsync(a => a.AnalysisRequestId == id);

            if (ar is null) return NotFound();

            var isAnalystOnly = User.IsInRole("Analyst") && !User.IsInRole("Admin");

            if (isAnalystOnly)
            {
                ar.Status = input.Status;
                ar.BuiltUpPercent = input.BuiltUpPercent;
                ar.GreenAreaM2 = input.GreenAreaM2;
                ar.HardenedAreaM2 = input.HardenedAreaM2;
                ar.TruckParkingSpots = input.TruckParkingSpots;
                ar.CarParkingSpots = input.CarParkingSpots;
                ar.HasArchaeologyRisk = input.HasArchaeologyRisk;
                ar.HasEarthworksRisk = input.HasEarthworksRisk;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id });
            }

            // ADMIN
            if (!ModelState.IsValid)
                return View(input);

            // Admin zmienia wejście
            ar.PlotWidthM = input.PlotWidthM;
            ar.PlotLengthM = input.PlotLengthM;
            ar.ModuleWidthM = input.ModuleWidthM;
            ar.ModuleLengthM = input.ModuleLengthM;

            // Admin może też poprawić adres (tekst) lub rozbite pola – zależnie jak masz widok Edit
            ar.AddressKind = input.AddressKind;
            ar.Country = input.Country;
            ar.City = input.City;
            ar.PostalCode = input.PostalCode;
            ar.Street = input.Street;
            ar.StreetNumber = input.StreetNumber;
            ar.PlotNumber = input.PlotNumber;
            ar.CadastralArea = input.CadastralArea;
            ar.Commune = input.Commune;

            ar.Address = BuildDisplayAddress(ar);

            // opcjonalnie: admin może też zmienić status
            ar.Status = input.Status;

            // auto przeliczenie wyników po zmianie wejścia
            _calc.ComputeAndApply(ar, new AnalysisAssumptions
            {
                GreenPercent = 0.25m,
                GeoGridsHardenedReduction = 0.0m
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // DELETE GET: Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return NotFound();

            var ar = await _db.AnalysisRequests
                .AsNoTracking()
                .Include(a => a.Variant)
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(a => a.AnalysisRequestId == id.Value);

            if (ar is null) return NotFound();

            return View(ar);
        }

        // DELETE POST: Admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ar = await _db.AnalysisRequests.FindAsync(id);
            if (ar is null) return RedirectToAction(nameof(Index));

            _db.AnalysisRequests.Remove(ar);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // RANKING: top analiz z BuiltUpPercent >= 40
        [Authorize(Roles = "Admin,Analyst")]
        public async Task<IActionResult> ProfitabilityRanking(int top = 10)
        {
            top = Math.Clamp(top, 1, 100);

            var data = await _db.AnalysisRequests
                .AsNoTracking()
                .Include(a => a.Variant)
                .Where(a => a.BuiltUpPercent != null && a.BuiltUpPercent >= 40)
                .OrderByDescending(a => a.BuiltUpPercent)
                .ThenByDescending(a => a.CreatedAt)
                .Take(top)
                .ToListAsync();

            return View(data);
        }

        // DASHBOARD: ostatnie 30 dni (per status i per wariant)
        [Authorize(Roles = "Admin,Analyst")]
        public async Task<IActionResult> Dashboard()
        {
            var from = DateTime.UtcNow.AddDays(-30);

            var byStatus = await _db.AnalysisRequests
                .AsNoTracking()
                .Where(a => a.CreatedAt >= from)
                .GroupBy(a => a.Status)
                .Select(g => new DashboardRow
                {
                    Label = g.Key.ToString(),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var byVariant = await _db.AnalysisRequests
                .AsNoTracking()
                .Where(a => a.CreatedAt >= from)
                .Include(a => a.Variant)
                .GroupBy(a => a.Variant.Name)
                .Select(g => new DashboardRow
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                FromUtc = from,
                ByStatus = byStatus,
                ByVariant = byVariant
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GeneratePdf(int id)
        {
            var ar = await _db.AnalysisRequests
                .Include(a => a.Variant)
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(a => a.AnalysisRequestId == id);

            if (ar is null) return NotFound();
            if (!CanAccess(ar)) return Forbid();

            var isAdminOrAnalyst = User.IsInRole("Admin") || User.IsInRole("Analyst");
            if (!isAdminOrAnalyst && ar.Variant != null && !ar.Variant.IncludesPdf)
                return Forbid();

            var computation = _calc.ComputeAndApply(ar);

            var bytes = _pdf.GenerateAnalysisRequestReport(ar, ar.Variant!, computation);
            var fileName = $"raport-analizy-{ar.AnalysisRequestId}.pdf";

            return File(bytes, "application/pdf", fileName);
        }

        // ---- helpers ----

        private bool CanAccess(AnalysisRequest ar)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Analyst"))
                return true;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return ar.ApplicationUserId == userId;
        }

        private static string BuildDisplayAddress(AnalysisRequest ar)
        {
            // Działka
            if (ar.AddressKind == AddressKind.Plot)
            {
                var s = $"Działka {ar.PlotNumber}";
                if (!string.IsNullOrWhiteSpace(ar.CadastralArea)) s += $", obręb {ar.CadastralArea}";
                if (!string.IsNullOrWhiteSpace(ar.Commune)) s += $", gmina {ar.Commune}";
                if (!string.IsNullOrWhiteSpace(ar.City)) s += $", {ar.City}";
                if (!string.IsNullOrWhiteSpace(ar.Country)) s += $", {ar.Country}";
                return s;
            }

            // Adres
            var parts = new List<string>();

            var streetPart = (ar.Street ?? "").Trim();
            var nrPart = (ar.StreetNumber ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(streetPart))
                parts.Add(string.IsNullOrWhiteSpace(nrPart) ? streetPart : $"{streetPart} {nrPart}");

            var cityPart = (ar.City ?? "").Trim();
            var postalPart = (ar.PostalCode ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(cityPart))
                parts.Add(string.IsNullOrWhiteSpace(postalPart) ? cityPart : $"{postalPart} {cityPart}");

            var countryPart = (ar.Country ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(countryPart))
                parts.Add(countryPart);

            // fallback — żeby Required na Address nie wywalał zapisu
            if (parts.Count == 0)
                return "—";

            return string.Join(", ", parts);
        }

        // proste VM do dashboardu
        public class DashboardViewModel
        {
            public DateTime FromUtc { get; set; }
            public List<DashboardRow> ByStatus { get; set; } = new();
            public List<DashboardRow> ByVariant { get; set; } = new();
        }

        public class DashboardRow
        {
            public string Label { get; set; } = "";
            public int Count { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GeneratePdf(int id)
        {
            var ar = await _db.AnalysisRequests
                .Include(a => a.Variant)
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(a => a.AnalysisRequestId == id);

            if (ar is null) return NotFound();
            if (!CanAccess(ar)) return Forbid();

            var isAdminOrAnalyst = User.IsInRole("Admin") || User.IsInRole("Analyst");

            // jeśli Variant nie załadował się (nie powinno, ale lepiej bezpiecznie)
            if (!isAdminOrAnalyst && (ar.Variant is null || !ar.Variant.IncludesPdf))
                return Forbid();
            // Dolicz wyniki jeśli potrzeba
            var computation = _calc.ComputeAndApply(ar);

            var bytes = _pdf.GenerateAnalysisRequestReport(ar, ar.Variant!, computation);
            var fileName = $"raport-analizy-{ar.AnalysisRequestId}.pdf";

            return File(bytes, "application/pdf", fileName);
        }

    }
}

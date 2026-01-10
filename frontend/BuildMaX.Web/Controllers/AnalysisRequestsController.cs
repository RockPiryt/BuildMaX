using System.Security.Claims;
using BuildMaX.Web.Data;
using BuildMaX.Web.Models.Domain;
using BuildMaX.Web.Models.Domain.Enums;
using BuildMaX.Web.Services.Analysis;
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

        public AnalysisRequestsController(AppDbContext db, IAnalysisCalculator calc)
        {
            _db = db;
            _calc = calc;
        }

        // LISTA: User widzi swoje; Admin/Analyst widzą wszystko
        public async Task<IActionResult> Index(AnalysisStatus? status, string? q)
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

            var data = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewData["SelectedStatus"] = status;
            ViewData["Query"] = q;

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
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Create(int variantId)
        {
            var variant = await _db.Variants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant is null) return NotFound();

            var ar = new AnalysisRequest
            {
                VariantId = variant.VariantId,
                Variant = variant
            };

            return View(ar);
        }

        // CREATE POST: user podaje tylko wymiary + adres, wariant jest hidden
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Create([Bind("VariantId,Address,PlotWidthM,PlotLengthM,ModuleWidthM,ModuleLengthM")] AnalysisRequest ar)
        {
            // walidacja bezpieczeństwa: czy wariant istnieje
            var variant = await _db.Variants.AsNoTracking().FirstOrDefaultAsync(v => v.VariantId == ar.VariantId);
            if (variant is null)
                ModelState.AddModelError(nameof(ar.VariantId), "Wybrany wariant nie istnieje.");

            if (!ModelState.IsValid)
            {
                // żeby widok mógł wyświetlić nazwę wariantu
                ar.Variant = variant!;
                return View(ar);
            }

            ar.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            ar.Status = AnalysisStatus.New;
            ar.CreatedAt = DateTime.UtcNow;

            // policz pola wyliczane
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
        // - Analyst: tylko status + pola wynikowe/ryzyka (jak było)
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
            {
                return View(input);
            }

            // Admin zmienia wejście
            ar.Address = input.Address;
            ar.PlotWidthM = input.PlotWidthM;
            ar.PlotLengthM = input.PlotLengthM;
            ar.ModuleWidthM = input.ModuleWidthM;
            ar.ModuleLengthM = input.ModuleLengthM;

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

        // ---- helpers ----

        private bool CanAccess(AnalysisRequest ar)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Analyst"))
                return true;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return ar.ApplicationUserId == userId;
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
    }
}

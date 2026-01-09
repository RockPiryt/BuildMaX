using System.Security.Claims;
using BuildMaX.Web.Data;
using BuildMaX.Web.Models.Domain;
using BuildMaX.Web.Models.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BuildMaX.Web.Controllers
{
    [Authorize]
    public class AnalysisRequestsController : Controller
    {
        private readonly AppDbContext _db;

        public AnalysisRequestsController(AppDbContext db)
        {
            _db = db;
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

        // CREATE: zwykły User + Admin (u Ciebie rola "User" jest seedowana)
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Create()
        {
            await PopulateVariantsSelectListAsync();
            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Create([Bind("VariantId,Address,PlotAreaM2,ModuleAreaM2")] AnalysisRequest ar)
        {
            if (!ModelState.IsValid)
            {
                await PopulateVariantsSelectListAsync(ar.VariantId);
                return View(ar);
            }

            ar.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            ar.Status = AnalysisStatus.New;
            ar.CreatedAt = DateTime.UtcNow;

            _db.AnalysisRequests.Add(ar);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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

            await PopulateVariantsSelectListAsync(ar.VariantId);
            return View(ar);
        }

        // EDIT POST:
        // - Analyst: tylko status + pola wynikowe/ryzyka
        // - Admin: pełna edycja
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Analyst")]
        public async Task<IActionResult> Edit(int id, AnalysisRequest input)
        {
            if (id != input.AnalysisRequestId) return NotFound();

            var ar = await _db.AnalysisRequests.FirstOrDefaultAsync(a => a.AnalysisRequestId == id);
            if (ar is null) return NotFound();

            var isAnalystOnly = User.IsInRole("Analyst") && !User.IsInRole("Admin");

            if (isAnalystOnly)
            {
                // Analyst: tylko "workflow"
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

            // Admin: walidujemy normalnie
            if (!ModelState.IsValid)
            {
                await PopulateVariantsSelectListAsync(input.VariantId);
                return View(input);
            }

            ar.VariantId = input.VariantId;
            ar.Address = input.Address;
            ar.PlotAreaM2 = input.PlotAreaM2;
            ar.ModuleAreaM2 = input.ModuleAreaM2;

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

        private async Task PopulateVariantsSelectListAsync(int? selectedVariantId = null)
        {
            var variants = await _db.Variants
                .AsNoTracking()
                .OrderBy(v => v.Price)
                .ThenBy(v => v.Name)
                .Select(v => new { v.VariantId, v.Name })
                .ToListAsync();

            ViewData["VariantId"] = new SelectList(variants, "VariantId", "Name", selectedVariantId);
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

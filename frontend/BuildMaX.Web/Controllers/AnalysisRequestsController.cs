using System.Security.Claims;
using BuildMaX.Web.Data;
using BuildMaX.Web.Models.Domain;
using BuildMaX.Web.Models.Domain.Enums;
using BuildMaX.Web.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

//(CRUD zleceń; role + LINQ)
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

        // GET: AnalysisRequests
        // Client: widzi swoje; Admin/Analyst: widzą wszystkie
        // LINQ: filtr po statusie + sort po CreatedAt + include Variant
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

        // GET: AnalysisRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();

            var ar = await _db.AnalysisRequests
                .AsNoTracking()
                .Include(a => a.Variant)
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(a => a.AnalysisRequestId == id.Value);

            if (ar is null) return NotFound();

            // Client nie może podejrzeć cudzych
            if (!CanAccessAnalysis(ar))
                return Forbid();

            return View(ar);
        }

        // GET: AnalysisRequests/Create
        [Authorize(Roles = "Client,Admin")] // Admin też może złożyć "testowe" zlecenie
        public async Task<IActionResult> Create()
        {
            await PopulateVariantsSelectListAsync();
            return View();
        }

        // POST: AnalysisRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client,Admin")]
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

            // Tu możesz później podpiąć "AnalysisCalculator" i uzupełniać wyniki
            // ar.BuiltUpPercent = ...
            // ar.GreenAreaM2 = ...

            _db.AnalysisRequests.Add(ar);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: AnalysisRequests/Edit/5
        // Admin: pełna edycja; Analyst: tylko status (możesz też rozdzielić na EditStatus)
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

        // POST: AnalysisRequests/Edit/5
        // Admin może edytować całość; Analyst tylko status i pola wynikowe/ryzyka (opcjonalnie)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Analyst")]
        public async Task<IActionResult> Edit(int id, AnalysisRequest input)
        {
            if (id != input.AnalysisRequestId) return NotFound();

            var ar = await _db.AnalysisRequests.FirstOrDefaultAsync(a => a.AnalysisRequestId == id);
            if (ar is null) return NotFound();

            if (User.IsInRole("Analyst") && !User.IsInRole("Admin"))
            {
                // Analyst: aktualizuje tylko status i ewentualnie wyniki/ryzyka
                ar.Status = input.Status;
                ar.BuiltUpPercent = input.BuiltUpPercent;
                ar.GreenAreaM2 = input.GreenAreaM2;
                ar.HardenedAreaM2 = input.HardenedAreaM2;
                ar.TruckParkingSpots = input.TruckParkingSpots;
                ar.CarParkingSpots = input.CarParkingSpots;
                ar.HasArchaeologyRisk = input.HasArchaeologyRisk;
                ar.HasEarthworksRisk = input.HasEarthworksRisk;

                // Nie dotykamy: VariantId, Address, PlotAreaM2, ModuleAreaM2, ApplicationUserId, CreatedAt
                ModelState.Clear(); // walidacja inputu niepotrzebna dla pól, których nie zapisujemy
            }
            else
            {
                // Admin: pełna edycja
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
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: AnalysisRequests/Delete/5
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

        // POST: AnalysisRequests/Delete/5
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

        // Własne LINQ: ranking opłacalności (top N analiz z BuiltUpPercent >= 40)
        [Authorize(Roles = "Admin,Analyst")]
        public async Task<IActionResult> ProfitabilityRanking(int top = 10)
        {
            if (top < 1) top = 10;
            if (top > 100) top = 100;

            var data = await _db.AnalysisRequests
                .AsNoTracking()
                .Include(a => a.Variant)
                .Include(a => a.ApplicationUser)
                .Where(a => a.BuiltUpPercent >= 40)
                .OrderByDescending(a => a.BuiltUpPercent)
                .ThenByDescending(a => a.CreatedAt)
                .Take(top)
                .ToListAsync();

            return View(data);
        }

        // Własne LINQ: dashboard (ilość analiz per status w ostatnich 30 dniach)
        [Authorize(Roles = "Admin,Analyst")]
        public async Task<IActionResult> Dashboard()
        {
            var from = DateTime.UtcNow.AddDays(-30);

            var data = await _db.AnalysisRequests
                .AsNoTracking()
                .Where(a => a.CreatedAt >= from)
                .GroupBy(a => a.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return View(data);
        }

        private bool CanAccessAnalysis(AnalysisRequest ar)
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
                .ToListAsync();

            ViewData["VariantId"] = new SelectList(variants, "VariantId", "Name", selectedVariantId);
        }
    }
}

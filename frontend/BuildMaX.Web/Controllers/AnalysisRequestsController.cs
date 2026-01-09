using BuildMaX.Web.Data;
using BuildMaX.Web.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

//(CRUD zleceń; role + LINQ)
namespace BuildMaX.Web.Controllers
{
    [Authorize]
    public class AnalysisRequestsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<BuildMaX.Web.Models.Identity.ApplicationUser> _users;

        public AnalysisRequestsController(AppDbContext db, UserManager<BuildMaX.Web.Models.Identity.ApplicationUser> users)
        {
            _db = db;
            _users = users;
        }

        private async Task<string> CurrentUserIdAsync()
        {
            var user = await _users.GetUserAsync(User);
            return user!.Id;
        }

        private bool IsAdmin() => User.IsInRole("Admin");

        // GET: /AnalysisRequests
        public async Task<IActionResult> Index()
        {
            var uid = await CurrentUserIdAsync();

            var query = _db.AnalysisRequests
                .Include(a => a.Variant)
                .AsQueryable();

            if (!IsAdmin())
                query = query.Where(a => a.ApplicationUserId == uid);

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(items);
        }

        // GET: /AnalysisRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var uid = await CurrentUserIdAsync();

            var query = _db.AnalysisRequests
                .Include(a => a.Variant)
                .AsQueryable();

            if (!IsAdmin())
                query = query.Where(a => a.ApplicationUserId == uid);

            var item = await query.FirstOrDefaultAsync(m => m.AnalysisRequestId == id);
            if (item == null) return NotFound();

            return View(item);
        }

        // GET: /AnalysisRequests/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Variants = await _db.Variants
                .OrderBy(v => v.Name)
                .ToListAsync();

            return View();
        }

        // POST: /AnalysisRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnalysisRequest model)
        {
            // użytkownik nie powinien wysyłać ApplicationUserId z formularza
            model.ApplicationUserId = await CurrentUserIdAsync();
            model.CreatedAt = DateTime.UtcNow;

            if (!ModelState.IsValid)
            {
                ViewBag.Variants = await _db.Variants.OrderBy(v => v.Name).ToListAsync();
                return View(model);
            }

            _db.AnalysisRequests.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /AnalysisRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var uid = await CurrentUserIdAsync();

            var query = _db.AnalysisRequests.AsQueryable();
            if (!IsAdmin())
                query = query.Where(a => a.ApplicationUserId == uid);

            var item = await query.FirstOrDefaultAsync(a => a.AnalysisRequestId == id);
            if (item == null) return NotFound();

            ViewBag.Variants = await _db.Variants.OrderBy(v => v.Name).ToListAsync();
            return View(item);
        }

        // POST: /AnalysisRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnalysisRequest model)
        {
            if (id != model.AnalysisRequestId) return NotFound();

            var uid = await CurrentUserIdAsync();

            var existing = await _db.AnalysisRequests.FirstOrDefaultAsync(a => a.AnalysisRequestId == id);
            if (existing == null) return NotFound();

            if (!IsAdmin() && existing.ApplicationUserId != uid) return Forbid();

            // zabezpieczenie: user nie zmienia właściciela
            model.ApplicationUserId = existing.ApplicationUserId;

            if (!ModelState.IsValid)
            {
                ViewBag.Variants = await _db.Variants.OrderBy(v => v.Name).ToListAsync();
                return View(model);
            }

            // mapowanie pól (żeby nie nadpisać pól systemowych)
            existing.VariantId = model.VariantId;
            existing.Address = model.Address;
            existing.PlotAreaM2 = model.PlotAreaM2;
            existing.ModuleAreaM2 = model.ModuleAreaM2;

            existing.BuiltUpPercent = model.BuiltUpPercent;
            existing.GreenAreaM2 = model.GreenAreaM2;
            existing.HardenedAreaM2 = model.HardenedAreaM2;

            existing.TruckParkingSpots = model.TruckParkingSpots;
            existing.CarParkingSpots = model.CarParkingSpots;

            existing.HasArchaeologyRisk = model.HasArchaeologyRisk;
            existing.HasEarthworksRisk = model.HasEarthworksRisk;

            // status tylko admin? jeśli chcesz:
            if (IsAdmin())
                existing.Status = model.Status;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /AnalysisRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var uid = await CurrentUserIdAsync();

            var query = _db.AnalysisRequests
                .Include(a => a.Variant)
                .AsQueryable();

            if (!IsAdmin())
                query = query.Where(a => a.ApplicationUserId == uid);

            var item = await query.FirstOrDefaultAsync(m => m.AnalysisRequestId == id);
            if (item == null) return NotFound();

            return View(item);
        }

        // POST: /AnalysisRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uid = await CurrentUserIdAsync();

            var item = await _db.AnalysisRequests.FirstOrDefaultAsync(a => a.AnalysisRequestId == id);
            if (item == null) return NotFound();

            if (!IsAdmin() && item.ApplicationUserId != uid) return Forbid();

            _db.AnalysisRequests.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

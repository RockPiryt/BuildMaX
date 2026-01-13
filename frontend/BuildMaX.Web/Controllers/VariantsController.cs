using BuildMaX.Web.Data;
using BuildMaX.Web.Models.Domain;
using BuildMaX.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

//(CRUD admin)
namespace BuildMaX.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VariantsController : Controller
    {
        private readonly AppDbContext _db;

        public VariantsController(AppDbContext db)
        {
            _db = db;
        }

        // GET: Variants
        public async Task<IActionResult> Index()
        {
            var variants = await _db.Variants
                .AsNoTracking()
                .OrderBy(v => v.VariantId)
                .ToListAsync();

            return View(variants);
        }

        // GET: Variants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();

            var variant = await _db.Variants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VariantId == id.Value);

            if (variant is null) return NotFound();

            return View(variant);
        }

        // GET: Variants/Create
        public IActionResult Create() => View();

        // POST: Variants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price,Description,IncludesPdf,IncludesPercentDetails,IncludesSitePlan")] Variant variant)
        {
            if (!ModelState.IsValid) return View(variant);

            _db.Variants.Add(variant);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Variants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();

            var variant = await _db.Variants.FindAsync(id.Value);
            if (variant is null) return NotFound();

            return View(variant);
        }

        // POST: Variants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VariantId,Name,Price,Description,IncludesPdf,IncludesPercentDetails,IncludesSitePlan")] Variant variant)
        {
            if (id != variant.VariantId) return NotFound();
            if (!ModelState.IsValid) return View(variant);

            try
            {
                _db.Update(variant);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _db.Variants.AnyAsync(v => v.VariantId == variant.VariantId);
                if (!exists) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Variants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return NotFound();

            var variant = await _db.Variants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VariantId == id.Value);

            if (variant is null) return NotFound();

            return View(variant);
        }

        // POST: Variants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var variant = await _db.Variants.FindAsync(id);
            if (variant is null) return RedirectToAction(nameof(Index));

            // RESTRICT: blokuj jeśli są AnalysisRequests
            var hasRequests = await _db.AnalysisRequests
                .AsNoTracking()
                .AnyAsync(a => a.VariantId == id);

            if (hasRequests)
            {
                ModelState.AddModelError(string.Empty,
                    "Nie można usunąć wariantu, ponieważ istnieją powiązane zlecenia analizy (AnalysisRequests).");
                return View("Delete", variant);
            }

            _db.Variants.Remove(variant);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Własne LINQ: ranking wariantów wg ilości analiz opłacalnych (>= 40%)
        public async Task<IActionResult> TopProfitable()
        {
            // Bezpiecznie: nie opieramy się o a.Variant.Name (nawigacja może być null / brak Include),
            // tylko robimy JOIN po VariantId.
            var data =
                await (from a in _db.AnalysisRequests.AsNoTracking()
                       where a.BuiltUpPercent >= 40
                       join v in _db.Variants.AsNoTracking()
                            on a.VariantId equals v.VariantId
                       group a by v.Name into g
                       select new TopProfitableVariantVm
                       {
                           Variant = g.Key,
                           Count = g.Count()
                       })
                      .OrderByDescending(x => x.Count)
                      .ToListAsync();

            return View(data);
        }
    }
}

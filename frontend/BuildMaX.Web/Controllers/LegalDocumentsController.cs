 using BuildMaX.Web.Data;
using BuildMaX.Web.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

 //(CRUD admin)
namespace BuildMaX.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LegalDocumentsController : Controller
    {
        private readonly AppDbContext _db;

        public LegalDocumentsController(AppDbContext db)
        {
            _db = db;
        }

        // GET: LegalDocuments
        // LINQ: filtry po kategorii i wariancie + sortowanie
        public async Task<IActionResult> Index(string? category, int? variantId)
        {
            var query = _db.LegalDocuments
                .AsNoTracking()
                .Include(d => d.Variant)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(d => d.Category == category);

            if (variantId.HasValue)
                query = query.Where(d => d.VariantId == variantId.Value);

            var docs = await query
                .OrderBy(d => d.Category)
                .ThenBy(d => d.Title)
                .ToListAsync();

            ViewData["Categories"] = await _db.LegalDocuments
                .AsNoTracking()
                .Where(d => d.Category != null && d.Category != "")
                .Select(d => d.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewData["Variants"] = new SelectList(
                await _db.Variants.AsNoTracking().OrderBy(v => v.Name).ToListAsync(),
                "VariantId",
                "Name",
                variantId
            );

            ViewData["SelectedCategory"] = category;
            ViewData["SelectedVariantId"] = variantId;

            return View(docs);
        }

        // Własne LINQ: grupowanie dokumentów po kategorii (np. pod accordion)
        public async Task<IActionResult> Grouped()
        {
            var grouped = await _db.LegalDocuments
                .AsNoTracking()
                .GroupBy(d => d.Category ?? "Inne")
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count(),
                    Items = g.OrderBy(x => x.Title).ToList()
                })
                .OrderBy(g => g.Category)
                .ToListAsync();

            return View(grouped);
        }

        // GET: LegalDocuments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();

            var doc = await _db.LegalDocuments
                .AsNoTracking()
                .Include(d => d.Variant)
                .FirstOrDefaultAsync(d => d.LegalDocumentId == id.Value);

            if (doc is null) return NotFound();

            return View(doc);
        }

        // GET: LegalDocuments/Create
        public async Task<IActionResult> Create()
        {
            await PopulateVariantsSelectListAsync();
            return View();
        }

        // POST: LegalDocuments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Url,Category,VariantId")] LegalDocument doc)
        {
            if (!ModelState.IsValid)
            {
                await PopulateVariantsSelectListAsync(doc.VariantId);
                return View(doc);
            }

            _db.LegalDocuments.Add(doc);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: LegalDocuments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();

            var doc = await _db.LegalDocuments.FindAsync(id.Value);
            if (doc is null) return NotFound();

            await PopulateVariantsSelectListAsync(doc.VariantId);
            return View(doc);
        }

        // POST: LegalDocuments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LegalDocumentId,Title,Url,Category,VariantId")] LegalDocument doc)
        {
            if (id != doc.LegalDocumentId) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateVariantsSelectListAsync(doc.VariantId);
                return View(doc);
            }

            try
            {
                _db.Update(doc);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _db.LegalDocuments.AnyAsync(d => d.LegalDocumentId == doc.LegalDocumentId);
                if (!exists) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: LegalDocuments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return NotFound();

            var doc = await _db.LegalDocuments
                .AsNoTracking()
                .Include(d => d.Variant)
                .FirstOrDefaultAsync(d => d.LegalDocumentId == id.Value);

            if (doc is null) return NotFound();

            return View(doc);
        }

        // POST: LegalDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doc = await _db.LegalDocuments.FindAsync(id);
            if (doc is null) return RedirectToAction(nameof(Index));

            _db.LegalDocuments.Remove(doc);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateVariantsSelectListAsync(int? selectedVariantId = null)
        {
            var variants = await _db.Variants
                .AsNoTracking()
                .OrderBy(v => v.Name)
                .ToListAsync();

            ViewData["VariantId"] = new SelectList(variants, "VariantId", "Name", selectedVariantId);
        }
    }
}

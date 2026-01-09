using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildMaX.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VariantsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TopProfitable()
        {
            var data = _db.AnalysisRequests
                .Where(a => a.BuiltUpPercent >= 40)
                .GroupBy(a => a.Variant.Name)
                .Select(g => new
                {
                    Variant = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return View(data);
        }

    }
}

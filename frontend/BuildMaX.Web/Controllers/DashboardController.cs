using BuildMaX.Web.Data;
using BuildMaX.Web.Models.Domain;
using BuildMaX.Web.Models.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildMaX.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<BuildMaX.Web.Models.Identity.ApplicationUser> _users;

        public DashboardController(AppDbContext db, UserManager<BuildMaX.Web.Models.Identity.ApplicationUser> users)
        {
            _db = db;
            _users = users;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _users.GetUserAsync(User);

            var userId = user!.Id;

            var stats = await _db.AnalysisRequests
                .Where(a => a.UserId == userId)
                .GroupBy(a => a.Status)
                .Select(g => new StatusStat
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var avgBuiltUp = await _db.AnalysisRequests
                .Where(a => a.UserId == userId && a.BuiltUpPercent.HasValue)
                .AverageAsync(a => a.BuiltUpPercent);

            var latest = await _db.AnalysisRequests
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Include(a => a.Variant)
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                StatusStats = stats,
                AverageBuiltUpPercent = avgBuiltUp,
                LatestRequests = latest
            };

            return View(vm);
        }
    }

    public class StatusStat
    {
        public AnalysisStatus Status { get; set; }
        public int Count { get; set; }
    }
}

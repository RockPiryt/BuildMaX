using System.Diagnostics;
using BuildMaX.Web.Data;
using Microsoft.AspNetCore.Mvc;
using BuildMaX.Web.Models;

namespace BuildMaX.Web.Controllers;
//(public: landing/pricing)
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _db;

    public HomeController(ILogger<HomeController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index() => View();

    public IActionResult Privacy() => View();

    public IActionResult Pricing()
    {
        var variants = _db.Variants.ToList();
        return View(variants);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

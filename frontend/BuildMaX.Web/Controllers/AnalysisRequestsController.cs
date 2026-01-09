using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildMaX.Web.Controllers
{
    [Authorize]
    public class AnalysisRequestsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

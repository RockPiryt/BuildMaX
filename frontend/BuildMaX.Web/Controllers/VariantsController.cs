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
    }
}

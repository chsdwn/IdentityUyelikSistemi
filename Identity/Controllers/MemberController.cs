using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    public class MemberController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
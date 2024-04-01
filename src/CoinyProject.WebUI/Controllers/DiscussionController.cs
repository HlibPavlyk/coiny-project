using Microsoft.AspNetCore.Mvc;

namespace CoinyProject.WebUI.Controllers
{
    public class DiscussionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace LabCollect.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateUsers()
        {

            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}

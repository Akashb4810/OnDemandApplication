using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OwnProducts.Models;

namespace OwnProducts.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public ActionResult Index() => View();

        public ActionResult Products() => View();

        public ActionResult Order(string product)
        {
            return View(new Order { Product = product });
        }

        [HttpPost]
        public ActionResult Order(Order order)
        {
            if (ModelState.IsValid)
                return RedirectToAction("ThankYou");

            return View(order);
        }

        public ActionResult ThankYou() => View();
    }
}

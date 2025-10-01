using LabCollect.Models;
using LabCollect.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LabCollect.Controllers
{
    public class LabOwnerDashboardController : Controller
    {
        
        private readonly IOwnerDashboardService _ownerDashboardService;
        public LabOwnerDashboardController(IOwnerDashboardService ownerDashboardService)
        {
            _ownerDashboardService = ownerDashboardService;
        }

        public IActionResult Index(DateTime? startDate, DateTime? endDate, string paymentReceivedBy)
        {
            if (!startDate.HasValue)
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            var viewModel = _ownerDashboardService.GetOwnerDashboardSummary(startDate, endDate, paymentReceivedBy);
            ViewBag.StartDate = viewModel.StartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = viewModel.EndDate.ToString("yyyy-MM-dd");
            ViewBag.PaymentReceivedBy = paymentReceivedBy;
            return View(viewModel);
        }

        [Route("Transactions")]
        public IActionResult Transactions(int assistantId, DateTime? startDate, DateTime? endDate, string paymentReceivedBy, int page = 1, int pageSize = 10)
        {
            if (!startDate.HasValue) startDate = DateTime.Today.AddDays(-30); // default last 30 days
            if (!endDate.HasValue) endDate = DateTime.Today;

            var transactions = _ownerDashboardService.GetAssistantPaymentTransactions(
                assistantId, startDate, endDate, paymentReceivedBy);

            // pagination
            int totalRecords = transactions.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var pagedList = transactions.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // pass values to view
            ViewBag.AssistantId = assistantId;
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            ViewBag.PaymentReceivedBy = paymentReceivedBy;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(pagedList);
        }

        [HttpPost]
        public IActionResult MarkReceivedByOwner(int transactionId, int assistantId, DateTime? startDate, DateTime? endDate, string paymentReceivedBy)
        {
            _ownerDashboardService.MarkReceivedByOwner(transactionId);
            return RedirectToAction("Transactions", new { assistantId, startDate, endDate, paymentReceivedBy });
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new UserViewModel();

            // Load dropdown data
            ViewBag.Roles = GetRoles();       // List<SelectListItem>
            ViewBag.AppTypes = GetAppTypes(); // List<SelectListItem>

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserViewModel model)
        {
            ViewBag.Roles = GetRoles();
            ViewBag.AppTypes = GetAppTypes();

            if (!ModelState.IsValid)
                return View(model);

           
            if (_ownerDashboardService.CreateUser(model, out int newUserId))
            {
                TempData["Success"] = $"User created successfully with ID {newUserId}";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Failed to create user. Username might already exist.");
            return View(model);
        }

        // Example: Fetch roles from DB
        private List<SelectListItem> GetRoles()
        {
            return new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "Owner" },
        new SelectListItem { Value = "2", Text = "Assistant" }
    };
        }

        // Example: Fetch app types from DB
        private List<SelectListItem> GetAppTypes()
        {
            return new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "Lab" },
        new SelectListItem { Value = "2", Text = "Dairy" }
    };
        }

    }
}

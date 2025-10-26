using System.Drawing.Printing;
using System.Security.Claims;
using LabCollect.Models;
using LabCollect.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using LabCollect.Repository.Implementation;
using Rotativa;
using Rotativa.AspNetCore;
using System.Threading.Tasks;


namespace LabCollect.Controllers
{
    public class LabOwnerDashboardController : Controller
    {

        private readonly IOwnerDashboardService _ownerDashboardService;
        private readonly IPaymentService _paymentService;
        public LabOwnerDashboardController(IOwnerDashboardService ownerDashboardService, IPaymentService paymentService)
        {
            _ownerDashboardService = ownerDashboardService;
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string paymentReceivedBy)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                return RedirectToAction("Login", "Account");

            if (!startDate.HasValue)
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            var viewModel =await _ownerDashboardService.GetOwnerDashboardSummary(startDate, endDate, paymentReceivedBy);
            if(paymentReceivedBy!=null)
            {
                viewModel.AssistantSummaries = viewModel.AssistantSummaries.Where(e => e.AssistantName != null && e.AssistantName.Contains(paymentReceivedBy, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            ViewBag.StartDate = viewModel.StartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = viewModel.EndDate.ToString("yyyy-MM-dd");
            ViewBag.PaymentReceivedBy = paymentReceivedBy;
            return View(viewModel);
        }

        [Route("Transactions")]
        public async Task<IActionResult> Transactions(int assistantId, DateTime? startDate, DateTime? endDate, string paymentReceivedBy,string paymentReceivedByName, int page = 1, int pageSize = 10)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                return RedirectToAction("Login", "Account");

            if (!startDate.HasValue) startDate = DateTime.Today.AddDays(-30); // default last 30 days
            if (!endDate.HasValue) endDate = DateTime.Today;

            var transactions =await _ownerDashboardService.GetAssistantPaymentTransactions(
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
            ViewBag.PaymentReceivedByName = paymentReceivedByName;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(pagedList);
        }

        [HttpPost]
        public async Task<IActionResult> MarkReceivedByOwner(int transactionId, int assistantId, DateTime? startDate, DateTime? endDate, string paymentReceivedBy)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                return RedirectToAction("Login", "Account");

            await _ownerDashboardService.MarkReceivedByOwner(transactionId);
            return RedirectToAction("Transactions", new { assistantId, startDate, endDate, paymentReceivedBy });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new UserViewModel();
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                return RedirectToAction("Login", "Account");
            // Load dropdown data
            ViewBag.Roles = GetRoles();       // List<SelectListItem>
            ViewBag.AppTypes = GetAppTypes(); // List<SelectListItem>

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                return RedirectToAction("Login", "Account");
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



        public async Task<IActionResult> ExportToPdf(DateTime? startDate, DateTime? endDate, string? paymentReceivedBy)
        {
            // 1. Get main dashboard data
            var model =await _ownerDashboardService.GetOwnerDashboardSummary(startDate, endDate, paymentReceivedBy);

            var paymentsDetails=await _paymentService.GetPaymentsByAssistant(0);
            // 2. Add patient-level details for each assistant
            foreach (var assistant in model.AssistantSummaries)
            {
                var payments = paymentsDetails.Where(e => e.AssistantId == assistant.AssistantId);

                if (startDate.HasValue)
                    payments = payments.Where(p => p.CreatedDate.Date >= startDate.Value.Date).ToList();
                if (endDate.HasValue)
                    payments = payments.Where(p => p.CreatedDate.Date <= endDate.Value.Date).ToList();

                assistant.PatientPayments = payments.ToList(); // add new property for PDF rendering
            }

            // 3. Render PDF
            return new ViewAsPdf("OwnerDashboardPdf", model)
            {
                FileName = "LabOwnerDashboard.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10)
            };

        }

        public async Task<IActionResult> ExportToPdfAsPaymentList(DateTime? startDate, DateTime? endDate, string? paymentReceivedBy)
        {
            // 1. Get main dashboard data
           // var model = _ownerDashboardService.GetOwnerDashboardSummary(startDate, endDate, paymentReceivedBy);

            var paymentsDetails =await _paymentService.GetPaymentsByAssistant(0);
            paymentsDetails.OrderByDescending(e => e.SampleId);
                if (startDate.HasValue)
                paymentsDetails = paymentsDetails.Where(p => p.CreatedDate.Date >= startDate.Value.Date).ToList();
                if (endDate.HasValue)
                paymentsDetails = paymentsDetails.Where(p => p.CreatedDate.Date <= endDate.Value.Date).ToList();


            // 3. Render PDF
            return new ViewAsPdf("OwnerDashbordPatientListPdf", paymentsDetails)
            {
                FileName = "LabOwnerDashboardPatientList.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10)
            };

        }

    }
}

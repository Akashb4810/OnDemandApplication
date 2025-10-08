using System.Drawing.Printing;
using System.Linq;
using System.Security.Claims;
using LabCollect.Models;
using LabCollect.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LabCollect.Controllers
{
    public class AssistantController : Controller
    {
        private readonly IPaymentService _paymentService;

        public AssistantController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        //public IActionResult Index()
        //{
        //    if (HttpContext.Session.GetString("UserId") == null)
        //        return RedirectToAction("Login", "Account");

        //    int assistantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
        //    var payments = _paymentService.GetPaymentsByAssistant(assistantId);
        //    return View(payments);
        //}
        public IActionResult Index(DateTime? fromDate, DateTime? toDate, string paymentMethod, string status,string patientName, int page = 1, int pageSize = 10)
        {
            int assistantId = 0;
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value==null)
                return RedirectToAction("Login", "Account");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim!=null)
            {
                 assistantId = int.Parse(userIdClaim); // safe to use
            }

            //int assistantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // 1. All payments (no filter)
            var allPayments = _paymentService.GetPaymentsByAssistant(assistantId);

            // 2. Apply filters
            var filteredPayments = allPayments;

            if (fromDate.HasValue)
                filteredPayments = filteredPayments.Where(p => p.CreatedDate.Date >= fromDate.Value.Date).ToList();

            if (toDate.HasValue)
                filteredPayments = filteredPayments.Where(p => p.CreatedDate.Date <= toDate.Value.Date).ToList();

            if (!string.IsNullOrEmpty(paymentMethod))
                filteredPayments = filteredPayments.Where(p => p.PaymentMethod == paymentMethod).ToList();

            if (!string.IsNullOrEmpty(status))
                filteredPayments = filteredPayments.Where(p => p.Status == status).ToList();

            if (!string.IsNullOrEmpty(patientName))
                filteredPayments = filteredPayments
                    .Where(p => !string.IsNullOrEmpty(p.PatientName) &&
                                p.PatientName.Contains(patientName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            //       filteredPayments = filteredPayments
            //.GroupBy(p => p.PatientId)
            //.Select(g =>
            //{
            //    // get the latest record in this group
            //    var latest = g.OrderByDescending(p => p.CreatedDate).FirstOrDefault();

            //    return new PaymentViewModel
            //    {
            //        PatientId = g.Key,
            //        Amount = g.Sum(p => p.Amount),                       
            //        Status = latest?.Status,                             
            //        PaymentMethod = latest?.PaymentMethod,               
            //        CreatedDate = latest?.CreatedDate ?? DateTime.MinValue, 
            //        AssistantId = latest?.AssistantId ?? 0,              
            //        AssistantName = latest?.AssistantName                
            //    };
            //})
            //.ToList();

            ViewBag.SelectedPaymentMethod = paymentMethod;
            ViewBag.SelectedStatus = status;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.PatientName = patientName;
            // 3. All Payments Totals
            ViewBag.UnpaidTotalAmount_All = allPayments.Sum(p => p.RemaingAmount);
            ViewBag.paidTotalAmount_All = allPayments.Sum(p => p.PaidAmount);
            ViewBag.TotalAmount_All = allPayments.Sum(p => p.Amount);
            ViewBag.PatientCount_All = allPayments.Count;
            ViewBag.CashCount_All = allPayments.Count(p => p.PaymentMethod == "Cash");
            ViewBag.OnlineCount_All = allPayments.Count(p => p.PaymentMethod == "Online");
            ViewBag.UnpaidCount_All = allPayments.Count(p => p.Status == "Unpaid");

            int totalRecords = filteredPayments.Count;

            // Pagination
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var pagedPayments = filteredPayments.OrderByDescending(e=>e.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Pass data to ViewBag
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            // 4. Filtered Payments Totals
            // ViewBag.TotalAmount_Filtered = filteredPayments.Where(e => e.AssistantId == assistantId).Sum(p => p.RemaingAmount);
            ViewBag.PatientCount_Filtered = filteredPayments.Where(e => e.AssistantId == assistantId).ToList().Count;
            ViewBag.CashCount_Filtered = filteredPayments.Where(e => e.AssistantId == assistantId).Count(p => p.PaymentMethod == "Cash");
            ViewBag.OnlineCount_Filtered = filteredPayments.Where(e => e.AssistantId == assistantId).Count(p => p.PaymentMethod == "Online");
            ViewBag.UnpaidCount_Filtered = filteredPayments.Where(e => e.AssistantId == assistantId).Count(p => p.Status == "Unpaid");

            return View(pagedPayments);
        }

        [HttpGet]
        public IActionResult CreatePayment()
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                return RedirectToAction("Login", "Account");
            return View(new PaymentPatientViewModel());
        }
        //[HttpPost]
        //public IActionResult CreatePayment(PaymentPatientViewModel model)
        //{
        //    if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value==null)
        //        return RedirectToAction("Login", "Account");

        //    model.AssistantId =int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value); 

        //    if (!ModelState.IsValid)
        //        return View(model);

        //    bool success = _paymentService.create(model);
        //    TempData["Msg"] = success ? "Payment saved successfully!" : "Error saving payment!";
        //    return RedirectToAction("Index");
        //}

        public IActionResult Dashboard(DateTime? fromDate, DateTime? toDate)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                return RedirectToAction("Login", "Account");

            int assistantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Default filters
            if (!fromDate.HasValue) fromDate = DateTime.Today;
            if (!toDate.HasValue) toDate = DateTime.Today;

            var summary = _paymentService.GetAssistantDashboardSummary(assistantId, fromDate.Value, toDate.Value);
            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(summary);
        }

        public IActionResult RemainingPayments(DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                return RedirectToAction("Login", "Account");

            int assistantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value); 

            if (!fromDate.HasValue) fromDate = DateTime.Today;
            if (!toDate.HasValue) toDate = DateTime.Today;

            var list = _paymentService.GetRemainingPaymentsByAssistant(assistantId, fromDate.Value, toDate.Value);

            // Pagination logic
            int totalRecords = list.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var pagedList = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pass filters + pagination to ViewBag
            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(pagedList);
        }

        //public IActionResult RemainingPayments(DateTime? fromDate, DateTime? toDate)
        //{
        //    int assistantId = HttpContext.Session.GetInt32("UserId") ?? 0;

        //    if (!fromDate.HasValue) fromDate = DateTime.Today;
        //    if (!toDate.HasValue) toDate = DateTime.Today;

        //    var list = _paymentService.GetRemainingPaymentsByAssistant(assistantId, fromDate.Value, toDate.Value);
        //    ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
        //    ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");

        //    return View(list);
        //}

        // Repeat similar for PaidPayments, OnlinePayments, CashPayments

        [Route("PaidPayments")]
        public IActionResult PaidPayments(DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                return RedirectToAction("Login", "Account");
            int assistantId = 0;
            int? userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (userId.HasValue)
            {
                assistantId = userId.Value;
            }

            if (!fromDate.HasValue) fromDate = DateTime.Today;
            if (!toDate.HasValue) toDate = DateTime.Today;

            var list = _paymentService.GetPaidPaymentsByAssistant(assistantId, fromDate, toDate);

            // Pagination logic
            int totalRecords = list.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var pagedList = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pass filters + pagination to ViewBag
            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(pagedList);
        }

        [Route("OnlinePayments")]
        public IActionResult OnlinePayments(DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                return RedirectToAction("Login", "Account");
            int assistantId = 0;
            int? userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (userId.HasValue)
            {
                assistantId = userId.Value;
            }

            if (!fromDate.HasValue) fromDate = DateTime.Today;
            if (!toDate.HasValue) toDate = DateTime.Today;

            var list = _paymentService.GetOnlinePaymentsByAssistant(assistantId, fromDate, toDate);

            // Pagination logic
            int totalRecords = list.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var pagedList = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pass filters + pagination to ViewBag
            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(pagedList);
        }

        [Route("CashPayments")]
        public IActionResult CashPayments(DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                return RedirectToAction("Login", "Account");
            int assistantId = 0;
            int? userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (userId.HasValue)
            {
                assistantId = userId.Value;
            }
            if (!fromDate.HasValue) fromDate = DateTime.Today;
            if (!toDate.HasValue) toDate = DateTime.Today;

            var list = _paymentService.GetCashPaymentsByAssistant(assistantId, fromDate, toDate);

            // pagination
            int totalRecords = list.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var pagedList = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // pass values to view
            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(pagedList);
        }
    }
}

using LabCollect.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LabCollect.Repository.Interface;
using System.Security.Claims;

namespace LabCollect.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IPatientService _patientService;

        public PaymentController(IPaymentService paymentService, IPatientService patientService)
        {
            _paymentService = paymentService;
            _patientService = patientService;
        }

        [HttpGet]
        public IActionResult Create(int patientId=0)
        {
            if(patientId>0 )
            {
                var patient = _patientService.GetPatientById(patientId);
                PaymentPatientViewModel paymentPatientViewModel= new PaymentPatientViewModel();
                paymentPatientViewModel.PatientId = patient.PatientId;
                paymentPatientViewModel.PatientName = patient.PatientName;
                paymentPatientViewModel.ContactNumber = patient.ContactNumber;
                paymentPatientViewModel.Address = patient.Address;
                paymentPatientViewModel.Email = patient.Email;
                paymentPatientViewModel.DateOfBirth = patient.DateOfBirth;
                paymentPatientViewModel.Gender = patient.Gender;
                //paymentPatientViewModel.SampleId = patient.SampleId;
                
                return View(paymentPatientViewModel);
            }
            return View();
        }

        [HttpPost]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PaymentPatientViewModel model)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get AssistantId from session
            int assistantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            model.AssistantId = assistantId;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Handle Test Image Upload
            if (model.TestImage != null && model.TestImage.Length > 0)
            {
                // Create folder if not exists
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/tests");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                // Unique file name
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.TestImage.FileName);
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.TestImage.CopyTo(stream);
                }

                // Save path (relative to wwwroot)
                model.TestImagePath = "/uploads/tests/" + fileName;
            }

            // Save Payment Data
            var result = _paymentService.create(model);
            if (!result)
            {
                ViewBag.Error = "Error saving payment. Please try again.";
                return View(model);
            }

            ViewBag.Message = "Payment saved successfully.";
            return RedirectToAction("Index", "Assistant");
        }


        public IActionResult Update(int paymentId)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int assistantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var paymentinfo=_paymentService.GetPaymentsByPaymentId(paymentId, assistantId);
            PaymentTransactionViewModel paymentTransactionViewModel = new PaymentTransactionViewModel();
            paymentTransactionViewModel.PaymentId = paymentId;
            paymentTransactionViewModel.TransactionDate = DateTime.Now;
            paymentTransactionViewModel.RemaingAmount = paymentinfo.RemaingAmount;
            //paymentTransactionViewModel.PaymentMethod = paymentinfo.PaymentMethod;
            
            return View(paymentTransactionViewModel);
        }

        [HttpPost]
        public IActionResult Update(PaymentTransactionViewModel model)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get AssistantId from session
            int assistantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            model.PaymentRecievedBY = assistantId.ToString();
            model.TransactionDate = DateTime.Now;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            model.RemaingAmount = model.RemaingAmount - model.PaidAmount;
            var result = _paymentService.Update(model);
            if (!result)
            {
                ViewBag.Error = "Error saving payment. Please try again.";
                return View(model);
            }
            ViewBag.Message = "Payment saved successfully.";
            return RedirectToAction("Index", "Assistant"); ;
        }

        [HttpGet]
        public JsonResult SearchPatients(string term)
        {
            var patients = _patientService.SearchPatientsByName(term);
            var result = patients.Select(p => new
            {
                label = p.PatientName,   // shown in dropdown
                value = p.PatientName,   // value inserted
                patientId = p.PatientId,
                contact = p.ContactNumber,
                email = p.Email,
                address = p.Address 
            }).ToList();

            return Json(result);
        }
    }
}

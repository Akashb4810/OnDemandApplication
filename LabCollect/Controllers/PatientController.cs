using LabCollect.Models;
using LabCollect.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LabCollect.Controllers
{
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UpdatePatient(int patientId)
        {
            Patient patient = new Patient();
            patient=_patientService.GetPatientById(patientId);
            return View(patient);
        }
        [HttpPost]
        public async Task<IActionResult>UpdatePatient(Patient patient)
        {
            if (!ModelState.IsValid)
                return View(patient);

            bool success = await _patientService.UpdatePatientAsync(patient);

            if (success)
            {
                TempData["SuccessMessage"] = "✅ Patient updated successfully!";
                return RedirectToAction("Index", "Assistant" );
            }
            else
            {
                ModelState.AddModelError("", "❌ Failed to update patient. Please try again.");
                return View(patient);
            }
        }
    }
}

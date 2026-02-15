using System.Drawing.Printing;
using System.Threading.Tasks;
using LabCollect.Models;
using LabCollect.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LabCollect.Controllers
{
    public class TestsController : Controller
    {
        //private readonly ILogger _logger;
        private readonly ITestsService _testService;
        public TestsController(ITestsService testService/*, ILogger logger*/)
        {
            _testService = testService;
            //_logger = logger;
        }

        public async Task<IActionResult> Index(string? testName, int page = 1, int pageSize = 10 /*, string category*/)
        {
            // Fetch all tests
            var laboratoryTests = await _testService.GetAllAsync();

            // Filter by Test Name if provided
            if (!string.IsNullOrEmpty(testName))
            {
                laboratoryTests = laboratoryTests
                    .Where(t => t.TestName.Contains(testName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filter by Category if needed
            /*
            if (!string.IsNullOrEmpty(category))
            {
                laboratoryTests = laboratoryTests
                    .Where(t => t.CategoryName != null && t.CategoryName.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            */

            // Pass filter values back to ViewBag to retain them in form
            ViewBag.TestName = testName;
            // ViewBag.SelectedCategory = category;


            // Optional: Dashboard info
            ViewBag.TotalTests_All = laboratoryTests.Count;
            ViewBag.TotalPrice_All = laboratoryTests.Sum(t => t.Price);
            int totalTests = laboratoryTests.Count;
            var pagedTests = laboratoryTests
                .OrderByDescending(t => t.CreatedDate)
            .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalTests / pageSize);
            ViewBag.TotalTests_All = totalTests;
            ViewBag.TotalPrice_All = laboratoryTests.Sum(t => t.Price);

            return View(laboratoryTests);
        }

        //public async Task<IActionResult> Index()
        //{
        //    List<LaboratoryTestModel> laboratoryTestModel = new List<LaboratoryTestModel>();
        //    laboratoryTestModel = await _testService.GetAllAsync();
        //    return View(laboratoryTestModel);
        //}

        [HttpGet]
        public async Task<IActionResult> Create(int? id = 0)
        {
            //ViewBag.Categories = _testService.GetCategories();
            LaboratoryTestModel laboratoryTestModel = null;
            if (id > 0)
            {
                laboratoryTestModel = await _testService.GetByIdAsync(id);
            }
            return View(laboratoryTestModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(LaboratoryTestModel model)
        {
            if (ModelState.IsValid)
            {
                bool result = false;

                if (model.TestId > 0)
                {
                    // Update existing test
                    result = await _testService.UpdateAsync(model); // Use UpdateAsync for updates
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Test updated successfully!";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    // Insert new test
                    result = await _testService.InsertAsync(model);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Test created successfully!";
                        return RedirectToAction("Index");
                    }
                }

                // If insert/update fails
                TempData["ErrorMessage"] = "Failed to save test.";
                return RedirectToAction("Index");
            }

            // Invalid model state
            TempData["ErrorMessage"] = "Invalid input data.";
            return RedirectToAction("Index");
        }

        //[HttpPost]
        //public async Task<IActionResult> Create(LaboratoryTestModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        bool result = false;
        //        if (model.TestId>0)
        //        {
        //            result = await _testService.InsertAsync(model);
        //            // ViewBag.Categories = _testService.GetCategories();
        //            if (result)
        //                return RedirectToAction("Index");//Ok(new { message = "Test Updated successfully" });

        //        }

        //         result = await _testService.InsertAsync(model);
        //        // ViewBag.Categories = _testService.GetCategories();
        //        if (result)
        //             return RedirectToAction("Index"); ;
        //    }
        //    return StatusCode(500, new { message = "Failed to create test" });
        //}
    }
}

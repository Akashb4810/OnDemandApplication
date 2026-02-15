using System.Threading.Tasks;
using LabCollect.Models;
using LabCollect.Repository.Implementation;
using LabCollect.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;

namespace LabCollect.Controllers
{
    public class InventoryController : Controller
    {
        //private readonly IInventoryService _repo ;
        //public InventoryController(IInventoryService repo)
        //{  _repo = repo; }
        //// Dashboard
        //public async Task<ActionResult> Index()
        //{
        //    var reagents = await _repo.GetAllReagents();

        //    var tasks = reagents.Select(async r =>
        //    {
        //        var lots = await _repo.GetLotsByReagent(r.ReagentId);
        //        var total = lots.Sum(l => l.Quantity);

        //        return new ReagentStockViewModel
        //        {
        //            Reagent = r,
        //            TotalQty = total,
        //            IsLow = r.AlertThreshold.HasValue && total <= r.AlertThreshold.Value
        //        };
        //    });

        //    var model = await Task.WhenAll(tasks);

        //    return View(model.ToList());
        //}


        //[HttpGet("Inventory/Reagents")]
        //public async Task<ActionResult> Reagents()
        //{
        //    var data = await _repo.GetAllReagents();
        //    return View(data);
        //}


        

        //public async Task<ActionResult> EditReagent(int id)
        //{
        //    var reagents = await _repo.GetAllReagents(); 
        //    var r = reagents.FirstOrDefault(x => x.ReagentId == id);  // BETTER: repo method by Id

        //    if (r == null)
        //        return NotFound();

        //    return View(r);
        //}

        //[HttpPost]
        //public async Task<ActionResult> EditReagent(Reagent model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //     _repo.UpdateReagent(model);

        //    return RedirectToAction("Reagents");
        //}

        //// ------------------ LOTS LIST ------------------
        //public async Task<ActionResult> Lots(int reagentId)
        //{
        //    ViewBag.ReagentId = reagentId;

        //    var lots = await _repo.GetLotsByReagent(reagentId);

        //    return View(lots);
        //}

        //// ------------------ ADD LOT ------------------
        //[HttpGet]
        //public IActionResult AddLot()
        //{
        //    List<ReagentLot> reagentLot = new List<ReagentLot>();
        //    return View(reagentLot);
        //}

        //[HttpPost]
        //public IActionResult AddLot(ReagentLot model)
        //{
        //    if (!ModelState.IsValid)
        //        return RedirectToAction("Lots", new { reagentId = model.ReagentId });

        //    _repo.AddLot(model);
        //    return RedirectToAction("Lots", new { reagentId = model.ReagentId });
        //}


        //// ------------------ TEST USAGE LIST ------------------
        //public async Task<ActionResult> TestUsage(int testId)
        //{
        //    ViewBag.TestId = testId;

        //    var list = await _repo.GetUsageByTest(testId);

        //    return View(list);
        //}

        //// ------------------ ADD TEST USAGE ------------------
        //public async Task<ActionResult> AddTestUsage(int testId)
        //{
        //    ViewBag.TestId = testId;

        //    var reagents = await _repo.GetAllReagents();

        //    ViewBag.Reagents = new SelectList(reagents, "ReagentId", "ReagentName");
        //    TestReagentUsageModel testReagentUsageModel = new TestReagentUsageModel();
        //    return View(testReagentUsageModel);
        //}

        //[HttpPost]
        //public async Task<ActionResult> AddTestUsage(TestReagentUsageModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var reagents = await _repo.GetAllReagents();
        //        ViewBag.Reagents = new SelectList(reagents, "ReagentId", "ReagentName");

        //        return View(model);
        //    }

        //     _repo.AddTestReagentUsage(model);

        //    return RedirectToAction("TestUsage", new { testId = model.TestId });
        //}

        //// ------------------ TEST CAPACITY ------------------
        //public async Task<ActionResult> TestCapacity(int testId)
        //{
        //    ViewBag.TestId = testId;

        //    var dt = await _repo.GetTestCapacity(testId);

        //    return View(dt);  // Razor renders DataTable
        //}
        private readonly IInventoryService _repo;
        private readonly ITestsService _testService;
        public InventoryController(IInventoryService repo, ITestsService testService )
        {
            _repo = repo;
            _testService = testService;
        }

        // -------------------------------------------------------
        // 1️⃣ INDEX – List all reagents with total stock + tests
        // -------------------------------------------------------
        public ActionResult Index()
        {
            var model = _repo.GetReagentIndex();
            return View(model);
        }
        public ActionResult AddReagent() => View(new Reagent());

        [HttpPost]
        public ActionResult AddReagent(Reagent model)
        {
            if (!ModelState.IsValid) return View(model);
            _repo.AddReagent(model);
            return RedirectToAction("Reagents");
        }
        public ActionResult ViewLots(int reagentId)
        {
            ViewBag.ReagentId = reagentId;
            var lots = _repo.GetLotsByReagent(reagentId);

            // attach reagent name for UI convenience
            // get reagent name (simple query)
            //using (var con = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            //using (var cmd = new System.Data.SqlClient.SqlCommand("SELECT Name FROM Reagents WHERE ReagentId = @id", con))
            //{
            //    cmd.Parameters.AddWithValue("@id", reagentId);
            //    con.Open();
            //    var nm = cmd.ExecuteScalar() as string;
            //    ViewBag.ReagentName = nm;
            //}

            return View(lots);
        }

        //[HttpGet]
        //public async Task<ActionResult> AddLot(int reagentId)
        //{
        //    var reagents = await _repo.GetAllReagents();
        //    var model = new ReagentLot { ReagentId = reagentId, ReceivedDate = DateTime.Now };
        //    // pass reagent name to view
        //    //using (var con = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
        //    //using (var cmd = new System.Data.SqlClient.SqlCommand("SELECT Name FROM Reagents WHERE ReagentId = @id", con))
        //    //{
        //    //    cmd.Parameters.AddWithValue("@id", reagentId);
        //    //    con.Open();
        //    //    var nm = cmd.ExecuteScalar() as string;
        //    //    ViewBag.ReagentName = nm;
        //    //}
        //    return View(model);
        //}

        [HttpGet]
        public async Task<ActionResult> AddLot()
        {
            var reagents = await Task.Run(() => _repo.GetAllReagents());
            ViewBag.Reagents = reagents;

            var model = new ReagentsLot
            {
                ReceivedDate = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> AddLot(ReagentsLot model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Reagents = _repo.GetAllReagents();
                return View(model);
            }

            // Insert Parent
            int lotId =await _repo.InsertLot(model);

            // Insert Children
            foreach (var d in model.Details)
            {
                d.LotId = lotId;
                _repo.AddLotDetail(d);
            }

            return RedirectToAction("ViewLots"); // Your list page
        }

        //[HttpPost]
        //public ActionResult AddLot(ReagentLot model)
        //{
        //    if (!ModelState.IsValid) return View(model);
        //    _repo.AddLot(model);
        //    return RedirectToAction("ViewLots", new { reagentId = model.ReagentId });
        //}

        [HttpPost]
        public ActionResult UpdateTestsDone(int lotId, int CompletedTests)
        {
            _repo.UpdateTestsDone(lotId, CompletedTests);
            var lot = _repo.GetLotById(lotId);
            return RedirectToAction("ViewLots", new { reagentId = lot.ReagentId });
        }

        public ActionResult DeleteLot(int id)
        {
            var lot = _repo.GetLotById(id);
            //if (lot == null) return HttpNotFound();
            //_repo.DeleteLot(id);
            return RedirectToAction("ViewLots", new { reagentId = lot.ReagentId });
        }

        public ActionResult TestsDonePerLot()
        {
            var dt = _repo.GetTestsDonePerLot();
            return View(dt); // create a Razor view that accepts DataTable
        }



        [HttpGet]
        public async Task<ActionResult> AddTestUsage(int testId)
        {
            var reagents = await _repo.GetAllReagents();
            var assigned = await _repo.GetAllTestReagents(testId);

            ViewBag.Reagents = reagents;
            ViewBag.Assigned = assigned;

            return View(new AddTestReagentUsageModel
            {
                TestId = testId
            });
        }


        
        [HttpPost]
        public async Task<ActionResult> AddTestUsage(AddTestReagentUsageModel model)
        {
            if (model.Reagents == null || !model.Reagents.Any())
            {
                ModelState.AddModelError("", "Select at least one reagent.");
                var reagents = await _repo.GetAllReagents();
                ViewBag.Reagents = reagents;
                return View(model);
            }
               var rowAffected= await _repo.AddOrUpdateTestReagentUsage(model);

            return RedirectToAction("Index", "Tests");
        }

    }
}

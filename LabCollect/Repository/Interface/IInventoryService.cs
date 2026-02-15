using System.Data;
using LabCollect.Models;

namespace LabCollect.Repository.Interface
{
    public interface IInventoryService
    {
        Task<int> AddReagent(Reagent r);
        //void UpdateReagent(Reagent r);
        Task<List<Reagent>> GetAllReagents();
        //Task<int> AddLot(ReagentLot lot);
        //Task<List<ReagentLot>> GetLotsByReagent(int reagentId);
        Task<int> AddOrUpdateTestReagentUsage(AddTestReagentUsageModel model);
        Task<List<TestReagentUsageModel>> GetUsageByTest(int testId);
        //Task<DataTable> GetTestCapacity(int testId);
        //Task<int> GetFinalTestCapacity(int testId);
        //void DeductStockForPaymentTest(int paymentTestId);
        List<ReagentIndexViewModel> GetReagentIndex();
        List<ReagentLot> GetLotsByReagent(int reagentId);
        ReagentLot GetLotById(int lotId);
        void AddLot(ReagentLot model);

        void UpdateTestsDone(int lotId, int completedTests);
        void DeductStockForPaymentTest(int paymentTestId);
        DataTable GetTestsDonePerLot();
        Task<List<TestReagentUsageViewModel>> GetAllTestReagents(int testId);
        Task<int> InsertLot(ReagentsLot model);
        Task AddLotDetail(ReagentLotDetail detail);
    }
}

using LabCollect.Models;

namespace LabCollect.Repository.Interface
{
    public interface IOwnerDashboardService
    {
        Task<OwnerDashboardViewModel> GetOwnerDashboardSummary(DateTime? startDate, DateTime? endDate, string paymentReceivedBy);
        Task<List<TransactionDetail>> GetAssistantPaymentTransactions(int assistantId, DateTime? startDate, DateTime? endDate, string paymentReceivedBy);
        Task  MarkReceivedByOwner(int transactionId);
        bool CreateUser(UserViewModel model, out int newUserId);
        Task<List<TransactionDetail>> GetAllPaymentTransactions(DateTime? startDate, DateTime? endDate);

    }

}

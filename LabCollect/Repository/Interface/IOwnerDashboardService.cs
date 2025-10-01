using LabCollect.Models;

namespace LabCollect.Repository.Interface
{
    public interface IOwnerDashboardService
    {
        OwnerDashboardViewModel GetOwnerDashboardSummary(DateTime? startDate, DateTime? endDate, string paymentReceivedBy);
        List<TransactionDetail> GetAssistantPaymentTransactions(int assistantId, DateTime? startDate, DateTime? endDate, string paymentReceivedBy);
        void MarkReceivedByOwner(int transactionId);
        bool CreateUser(UserViewModel model, out int newUserId);
    }

}

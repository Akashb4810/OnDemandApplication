using LabCollect.Models;

namespace LabCollect.Repository.Interface
{
    public interface IPaymentService
    {
        Task<bool> create(PaymentPatientViewModel paymentViewModel);
        Task<bool> Update(PaymentTransactionViewModel paymentViewModel);
        Task<List<PaymentViewModel>> GetPaymentsByAssistant(int assistantId);
        Task<PaymentViewModel> GetPaymentsByPaymentId(int paymentId, int assistantId);
        Task<AssistantDashboardViewModel> GetAssistantDashboardSummary(int assistantId, DateTime? fromDate, DateTime? toDate);
        Task<List<PaymentViewModel>> GetRemainingPaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate);
        Task<List<PaymentViewModel>> GetPaidPaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate);
        Task<List<PaymentViewModel>> GetOnlinePaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate);
        Task<List<PaymentViewModel>> GetCashPaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate);
    }
}

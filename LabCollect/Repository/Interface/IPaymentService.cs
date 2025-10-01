using LabCollect.Models;

namespace LabCollect.Repository.Interface
{
    public interface IPaymentService
    {
        bool create(PaymentPatientViewModel paymentViewModel);
        bool Update(PaymentTransactionViewModel paymentViewModel);
        List<PaymentViewModel> GetPaymentsByAssistant(int assistantId);
        PaymentViewModel GetPaymentsByPaymentId(int paymentId, int assistantId);
        AssistantDashboardViewModel GetAssistantDashboardSummary(int assistantId, DateTime? fromDate, DateTime? toDate);
        List<PaymentViewModel> GetRemainingPaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate);
        List<PaymentViewModel> GetPaidPaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate);
        List<PaymentViewModel> GetOnlinePaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate);
        List<PaymentViewModel> GetCashPaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate);
    }
}

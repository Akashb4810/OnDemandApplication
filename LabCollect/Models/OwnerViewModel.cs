namespace LabCollect.Models
{
    public class OwnerViewModel
    {
    }
    public class AssistantPaymentSummary
    {
        public int AssistantId { get; set; }
        public string AssistantName { get; set; }
        public int PatientCount { get; set; }
        public decimal TotalPaymentCollected { get; set; }
        public decimal TotalRemainingAmount { get; set; }
        public decimal TotalReceivedByOwner { get; set; }
        public decimal TotalUnpaidToOwner { get; set; }
    }

    public class OwnerDashboardViewModel
    {
        public List<AssistantPaymentSummary> AssistantSummaries { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal TotalRemaining { get; set; }
        public decimal TotalUnpaidToOwner { get; set; }
        public decimal TotalReceivedByOwner { get; set; }
        public int TotalPatients { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PaymentReceivedBy { get; set; }
    }

    public class TransactionDetail
    {
        public int TransactionId { get; set; }
        public int PaymentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
        public DateTime TransactionDate { get; set; }
        public string PaymentRecivedBy { get; set; }
        public bool IsReceivedByOwner { get; set; }
        public decimal RemainingAmount { get; set; }
    }

    public class UserViewModel
    {
        public string UserName { get; set; }          // Required
        public string PasswordHash { get; set; }      // Required
        public int RoleId { get; set; }               // Required
        public int AppTypeId { get; set; }            // Required
        public string? FullName { get; set; }         // Optional
        public string? Email { get; set; }            // Optional
        public string? PhoneNumber { get; set; }      // Optional
        public bool IsActive { get; set; } = true;    // Default true
    }

}

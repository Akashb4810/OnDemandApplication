using System.ComponentModel.DataAnnotations;

namespace LabCollect.Models
{
    public class PaymentViewModel
    {
        public int PaymentId { get; set; }
        [Required]
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string? ContactNumber { get; set; }
        public string? Address { get; set; }

        [Required]
        public decimal Amount { get; set; }
        public decimal RemaingAmount { get; set; }
        public decimal PaidAmount { get; set; }

        [Required]
        public string Status { get; set; } // Paid/Unpaid
        public string? PaymentMethod { get; set; } = null;// Cash/Online

        public int AssistantId { get; set; } // Will be filled from Session
        public int SampleId { get; set; } // Will be filled from Session
        public DateTime CreatedDate { get; set; }
        public string? AssistantName { get; set; }
        public string? PrescriptionImgURL { get; set; }
        public int Visit { get; set; }
    }


    public class PaymentTransactionViewModel
    {
        [Key]
        public int TransactionId { get; set; }
        public int PaymentId { get; set; }
        public decimal RemaingAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Notes { get; set; }
        public string? PaymentRecievedBY { get; set; }
        public int PatientId { get; set; }
    }

    public class AssistantDashboardViewModel
    {
        public int TotalPatients { get; set; }
        public decimal TotalRemaining { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOnline { get; set; }
        public decimal TotalCash { get; set; }
    }

    public class    PaymentPatientViewModel
    {
        public int? PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ContactNumber { get; set; }
        public string? Email { get; set; }
        public string Address { get; set; }

        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string Status { get; set; }
        public int? AssistantId { get; set; }
        public int SampleId { get; set; }

        //public IFormFile TestImage { get; set; }
        //public string? TestImagePath { get; set; }
        public string Notes { get; set; }

    }
    public class Patient
    {
        public int PatientId { get; set; }           // Primary Key
        public int SampleId { get; set; }           // Primary Key
        public string PatientName { get; set; }      // Patient full name
        public DateTime? DateOfBirth { get; set; }   // Optional
        public string Gender { get; set; }           // Optional (Male/Female/Other)
        public string ContactNumber { get; set; }    // Optional
        public string Email { get; set; }            // Optional
        public string Address { get; set; }          // Optional

        // Optional helper properties if needed
        // public int Age => DateOfBirth.HasValue ? DateTime.Now.Year - DateOfBirth.Value.Year : 0;
    }

}

using LabCollect.Models;
using LabCollect.Repository.Interface;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LabCollect.Repository.Implementation
{
    public class OwnerDashboardService : IOwnerDashboardService
    {
        private readonly string _connectionString;
        public OwnerDashboardService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public OwnerDashboardViewModel GetOwnerDashboardSummary(DateTime? startDate, DateTime? endDate, string paymentReceivedBy)
        {
            var summaries = new List<AssistantPaymentSummary>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetOwnerDashboardSummary", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PaymentReceivedBy", string.IsNullOrEmpty(paymentReceivedBy) ? (object)DBNull.Value : paymentReceivedBy);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        summaries.Add(new AssistantPaymentSummary
                        {
                            AssistantId = reader.GetInt32(reader.GetOrdinal("AssistantId")),
                            AssistantName = reader.GetString(reader.GetOrdinal("AssistantName")),
                            PatientCount = reader.IsDBNull(reader.GetOrdinal("PatientCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("PatientCount")),
                            TotalPaymentCollected = reader.IsDBNull(reader.GetOrdinal("TotalPaymentCollected")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalPaymentCollected")),
                            TotalRemainingAmount = reader.IsDBNull(reader.GetOrdinal("TotalRemainingAmount")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalRemainingAmount")),
                            TotalReceivedByOwner = reader.IsDBNull(reader.GetOrdinal("TotalReceivedByOwner")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalReceivedByOwner")),
                            TotalUnpaidToOwner = reader.IsDBNull(reader.GetOrdinal("TotalUnpaidToOwner")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalUnpaidToOwner"))

                        });
                    }
                }
            }

            return new OwnerDashboardViewModel
            {
                AssistantSummaries = summaries,
                TotalPayment = summaries.Sum(x => x.TotalPaymentCollected),
                TotalRemaining = summaries.Sum(x => x.TotalRemainingAmount),
                TotalPatients = summaries.Sum(x => x.PatientCount),
                TotalReceivedByOwner = summaries.Sum(x => x.TotalReceivedByOwner),
                TotalUnpaidToOwner = summaries.Sum(x => x.TotalUnpaidToOwner),
                StartDate = startDate ?? DateTime.Today,
                EndDate = endDate ?? DateTime.Today,
                PaymentReceivedBy = paymentReceivedBy
            };
        }

        public List<TransactionDetail> GetAssistantPaymentTransactions(int assistantId, DateTime? startDate, DateTime? endDate, string paymentReceivedBy)
        {      
            var transactions = new List<TransactionDetail>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetAssistantPaymentTransactions", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AssistantId", assistantId);
                cmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PaymentReceivedBy", string.IsNullOrEmpty(paymentReceivedBy) ? (object)DBNull.Value : paymentReceivedBy);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new TransactionDetail
                        {
                            TransactionId = reader.IsDBNull(reader.GetOrdinal("TransactionId"))
    ? 0 : reader.GetInt32(reader.GetOrdinal("TransactionId")),

                            PaymentId = reader.IsDBNull(reader.GetOrdinal("PaymentId"))
    ? 0 : reader.GetInt32(reader.GetOrdinal("PaymentId")),

                            PatientId = reader.IsDBNull(reader.GetOrdinal("PatientId"))
    ? 0 : reader.GetInt32(reader.GetOrdinal("PatientId")),

                            PatientName = reader.IsDBNull(reader.GetOrdinal("PatientName"))
    ? string.Empty : reader.GetString(reader.GetOrdinal("PatientName")),

                            PaidAmount = reader.IsDBNull(reader.GetOrdinal("PaidAmount"))
    ? 0 : reader.GetDecimal(reader.GetOrdinal("PaidAmount")),

                            PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod"))
    ? string.Empty : reader.GetString(reader.GetOrdinal("PaymentMethod")),

                            Notes = reader.IsDBNull(reader.GetOrdinal("Notes"))
    ? null : reader.GetString(reader.GetOrdinal("Notes")),

                            TransactionDate = reader.IsDBNull(reader.GetOrdinal("TransactionDate"))
    ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("TransactionDate")),

                            PaymentRecivedBy = reader.IsDBNull(reader.GetOrdinal("PaymentRecivedBy"))
    ? string.Empty : reader.GetString(reader.GetOrdinal("PaymentRecivedBy")),

                            IsReceivedByOwner = reader.IsDBNull(reader.GetOrdinal("IsReceivedByOwner"))
    ? false : reader.GetBoolean(reader.GetOrdinal("IsReceivedByOwner")),

                            RemainingAmount = reader.IsDBNull(reader.GetOrdinal("RemainingAmount"))
    ? 0 : reader.GetDecimal(reader.GetOrdinal("RemainingAmount")),

                            TotalAmount = reader.IsDBNull(reader.GetOrdinal("TotalAmount"))
    ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                            DiscountAmount = reader.IsDBNull(reader.GetOrdinal("DiscountAmount"))
    ? 0 : reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),

                            BillAmount = reader.IsDBNull(reader.GetOrdinal("Amount"))
    ? 0 : reader.GetDecimal(reader.GetOrdinal("Amount"))

                        });
                    }
                }
            }

            return transactions;
        }

        public void MarkReceivedByOwner(int transactionId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("UPDATE PaymentTransactions SET IsReceivedByOwner = 1 WHERE TransactionId = @TransactionId", conn))
            {
                cmd.Parameters.AddWithValue("@TransactionId", transactionId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool CreateUser(UserViewModel model, out int newUserId)
        {
            newUserId = 0; // default if fails

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_CreateUser", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Input parameters – handle NULLs with DBNull.Value
                cmd.Parameters.AddWithValue("@UserName", model.UserName);
                cmd.Parameters.AddWithValue("@PasswordHash", model.PasswordHash);
                cmd.Parameters.AddWithValue("@RoleId", model.RoleId);
                cmd.Parameters.AddWithValue("@AppTypeId", model.AppTypeId);
                cmd.Parameters.AddWithValue("@FullName", (object?)model.FullName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object?)model.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PhoneNumber", (object?)model.PhoneNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                try
                {
                    conn.Open();

                    // Execute and get the returned UserId
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        newUserId = Convert.ToInt32(result);
                        return true;
                    }
                    return false;
                }
                catch (SqlException ex)
                {
                    // Optionally, log ex.Message
                    // This will catch the "Username already exists" RAISERROR
                    return false;
                }
            }
        }

    }

}

using System.Data;
using LabCollect.Models;
using LabCollect.Repository.Interface;
using Microsoft.Data.SqlClient;

namespace LabCollect.Repository.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly string _connectionString;

        public PaymentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }



        public bool create(PaymentPatientViewModel model)
        {
           // newPaymentId = 0; // default if fails

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_InsertPayment", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Input parameters – handle NULLs with DBNull.Value
                cmd.Parameters.AddWithValue("@PatientId", (object?)model.PatientId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PatientName", (object?)model.PatientName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateOfBirth", (object?)model.DateOfBirth ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", (object?)model.Gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ContactNumber", (object?)model.ContactNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object?)model.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object?)model.Address ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@Amount", model.Amount);
                cmd.Parameters.AddWithValue("@PaidAmount", model.PaidAmount); // even if 0 it’s required
                cmd.Parameters.AddWithValue("@TotalAmount", model.TotalAmount); // even if 0 it’s required
                cmd.Parameters.AddWithValue("@DiscountAmount", model.DiscountAmount); // even if 0 it’s required
                cmd.Parameters.AddWithValue("@PaymentMethod", (object?)model.PaymentMethod ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", model.Status);
                cmd.Parameters.AddWithValue("@AssistantId", (object?)model.AssistantId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SampleId", (object?)model.SampleId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PrescriptionImgURL", model.TestImagePath);
                cmd.Parameters.AddWithValue("@Notes", model.Notes);

                // Output parameters
                var isSuccessParam = new SqlParameter("@IsSuccess", SqlDbType.Bit)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(isSuccessParam);

                var newPaymentIdParam = new SqlParameter("@NewPaymentId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(newPaymentIdParam);

                conn.Open();
                cmd.ExecuteNonQuery();

                // Read output parameters
                bool isSuccess = Convert.ToBoolean(isSuccessParam.Value);
                if (newPaymentIdParam.Value != DBNull.Value)
                    //newPaymentId = Convert.ToInt32(newPaymentIdParam.Value);

                return isSuccess;
            }
            return false;
        }

        public List<PaymentViewModel> GetPaymentsByAssistant(int assistantId)
        {
            var list = new List<PaymentViewModel>();

            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("sp_GetPaymentsByAssistant", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AssistantId", assistantId);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new PaymentViewModel
                {
                    PatientId = reader["PatientId"] != DBNull.Value ? Convert.ToInt32(reader["PatientId"]) : 0,
                    PaymentId = reader["PaymentId"] != DBNull.Value ? Convert.ToInt32(reader["PaymentId"]) : 0,
                    SampleId = reader["SampleId"] != DBNull.Value ? Convert.ToInt32(reader["SampleId"]) : 0,
                    Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : 0m,
                    PaymentMethod = reader["PaymentMethod"] != DBNull.Value ? reader["PaymentMethod"].ToString() : string.Empty,
                    Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : string.Empty,
                    AssistantId = assistantId,
                    PaidAmount = reader["PaidAmount"] != DBNull.Value ? Convert.ToDecimal(reader["PaidAmount"]) : 0m,
                    RemaingAmount = reader["RemainingAmount"] != DBNull.Value ? Convert.ToDecimal(reader["RemainingAmount"]) : 0m,
                    AssistantName = reader["AssistantName"] != DBNull.Value ? reader["AssistantName"].ToString() : string.Empty,
                    PatientName = reader["PatientName"] != DBNull.Value ? reader["PatientName"].ToString() : string.Empty,
                    ContactNumber = reader["ContactNumber"] != DBNull.Value ? reader["ContactNumber"].ToString() : string.Empty,
                    Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : string.Empty,
                    CreatedDate = reader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedDate"]) : DateTime.MinValue,
                    PrescriptionImgURL = reader["PrescriptionImgURL"] != DBNull.Value ? reader["PrescriptionImgURL"].ToString() : string.Empty,
                });
            }
            return list;
        }

        public PaymentViewModel GetPaymentsByPaymentId(int paymentId,int assistantId)
        {
            var paymentViewModel = new PaymentViewModel();

            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("sp_GetPaymentById", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PaymentId", paymentId);
            //cmd.Parameters.AddWithValue("@SampleId", paymentId);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                paymentViewModel = new PaymentViewModel
                {
                    PaymentId = reader["PaymentId"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("PaymentId")) : 0,
                    PatientId = reader["PatientId"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("PatientId")) : 0,
                    Amount = reader["Amount"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("Amount")) : 0m,
                    RemaingAmount = reader["RemainingAmount"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("RemainingAmount")) : 0m,
                    PaymentMethod = reader["PaymentMethod"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("PaymentMethod")) : string.Empty,
                    Status = reader["Status"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("Status")) : string.Empty,
                    CreatedDate = reader["CreatedDate"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("CreatedDate")) : DateTime.MinValue,
                    AssistantId = reader["AssistantId"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("AssistantId")) : 0,
                    AssistantName = reader["AssistantName"] != DBNull.Value ? reader["AssistantName"].ToString() : string.Empty,
                    SampleId = reader["SampleId"] != DBNull.Value ? Convert.ToInt32(reader["SampleId"]) : 0,

                };
            }
            return paymentViewModel;
        }

        public bool Update(PaymentTransactionViewModel paymentViewModel)
        {
           int newTransactionId = 0; // initialize output

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_AddPaymentTransaction", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Input parameters
                    cmd.Parameters.AddWithValue("@PaymentId", paymentViewModel.PaymentId);
                    cmd.Parameters.AddWithValue("@PaidAmount", paymentViewModel.PaidAmount);

                    cmd.Parameters.AddWithValue("@PaymentMethod",
                        (object?)paymentViewModel.PaymentMethod ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@Notes",
                        (object?)paymentViewModel.Notes ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@PaymentRecivedBy",
                        (object?)paymentViewModel.PaymentRecievedBY ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@TransactionDate",
                        paymentViewModel.TransactionDate);

                    // Output parameters
                    SqlParameter newTransactionParam = new SqlParameter("@NewTransactionId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(newTransactionParam);

                    SqlParameter isSuccessParam = new SqlParameter("@IsSuccess", SqlDbType.Bit)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(isSuccessParam);

                    // Execute
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    // Get outputs
                    bool isSuccess = (isSuccessParam.Value != DBNull.Value) &&
                                     Convert.ToBoolean(isSuccessParam.Value);

                    if (newTransactionParam.Value != DBNull.Value)
                    {
                        newTransactionId = Convert.ToInt32(newTransactionParam.Value);
                    }

                    return isSuccess;
                }
            }
        }

        public AssistantDashboardViewModel GetAssistantDashboardSummary(int assistantId, DateTime? fromDate, DateTime? toDate)
        {
            var model = new AssistantDashboardViewModel();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetAssistantDashboardSummary", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AssistantId", assistantId);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) model.TotalPatients = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    reader.NextResult();
                    if (reader.Read()) model.TotalRemaining = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    reader.NextResult();
                    if (reader.Read()) model.TotalPaid = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    reader.NextResult();
                    if (reader.Read()) model.TotalOnline = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    reader.NextResult();
                    if (reader.Read()) model.TotalCash = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                }
            }
            return model;
        }


        public List<PaymentViewModel> GetRemainingPaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate)
     => GetPayments("sp_GetRemainingPaymentsByAssistant", assistantId, fromDate, toDate);

        public List<PaymentViewModel> GetPaidPaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate)
            => GetPayments("sp_GetPaidPaymentsByAssistant", assistantId, fromDate, toDate);

        public List<PaymentViewModel> GetOnlinePaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate)
            => GetPayments("sp_GetOnlinePaymentsByAssistant", assistantId, fromDate, toDate);

        public List<PaymentViewModel> GetCashPaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate)
            => GetPayments("sp_GetCashPaymentsByAssistant", assistantId, fromDate, toDate);

        private List<PaymentViewModel> GetPayments(string spName, int assistantId, DateTime? fromDate, DateTime? toDate)
        {
            var list = new List<PaymentViewModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(spName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AssistantId", assistantId);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new PaymentViewModel
                        {
                            PaymentId = (int)reader["PaymentId"],
                            PatientId = (int)reader["PatientId"],
                            PatientName = reader["PatientName"].ToString(),
                            Amount = (decimal)reader["Amount"],
                            PaymentMethod = reader["PaymentMethod"].ToString(),
                            Status = reader["Status"].ToString(),
                            CreatedDate = (DateTime)reader["CreatedDate"],
                            //AssistantId = (int)reader["PaymentRecivedBy"],
                            PaidAmount = reader["PaidAmount"] as decimal? ?? 0,
                            RemaingAmount = reader["RemainingAmount"] as decimal? ?? 0,
                            Visit = reader["Visit"] as int? ?? 0
                        });
                    }
                }
            }
            return list;
        }

        public List<PaymentTransactionViewModel> GetOnlinePaymentsByAssistant(int assistantId)
            => GetTransactions("sp_GetOnlinePaymentsByAssistant", assistantId);

        public List<PaymentTransactionViewModel> GetCashPaymentsByAssistant(int assistantId)
            => GetTransactions("sp_GetCashPaymentsByAssistant", assistantId);

        private List<PaymentTransactionViewModel> GetTransactions(string spName, int assistantId)
        {
            var list = new List<PaymentTransactionViewModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(spName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AssistantId", assistantId);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new PaymentTransactionViewModel
                        {
                            TransactionId = (int)reader["TransactionId"],
                            PaymentId = (int)reader["PaymentId"],
                            PatientId = (int)reader["PatientId"],
                            PaidAmount = (decimal)reader["PaidAmount"],
                            PaymentMethod = reader["PaymentMethod"].ToString(),
                            Notes = reader["Notes"].ToString(),
                            TransactionDate = (DateTime)reader["TransactionDate"],
                            PaymentRecievedBY = reader["PaymentRecivedBy"].ToString(),
                        });
                    }
                }
            }
            return list;
        }
    }
}

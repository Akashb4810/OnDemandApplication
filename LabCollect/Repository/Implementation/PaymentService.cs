using System.Data;
using System.Threading.Tasks;
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


        public async Task<bool> create(PaymentPatientViewModel model)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction(); // ✅ Start transaction

                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_InsertPayment", conn, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // ✅ Input parameters
                        cmd.Parameters.AddWithValue("@PatientId", (object?)model.PatientId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PatientName", (object?)model.PatientName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DateOfBirth", (object?)model.DateOfBirth ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Gender", (object?)model.Gender ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactNumber", (object?)model.ContactNumber ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Email", (object?)model.Email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Address", (object?)model.Address ?? DBNull.Value);

                        cmd.Parameters.AddWithValue("@Amount", model.Amount);
                        cmd.Parameters.AddWithValue("@PaidAmount", model.PaidAmount);
                        cmd.Parameters.AddWithValue("@TotalAmount", model.TotalAmount);
                        cmd.Parameters.AddWithValue("@DiscountAmount", (object?)model.DiscountAmount ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PaymentMethod", (object?)model.PaymentMethod ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", model.Status);
                        cmd.Parameters.AddWithValue("@AssistantId", (object?)model.AssistantId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SampleId", (object?)model.SampleId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PrescriptionImgURL", model.TestImagePath);
                        cmd.Parameters.AddWithValue("@Notes", model.Notes ?? "");

                        // ✅ Output params
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

                        // Insert Payment
                        await cmd.ExecuteNonQueryAsync();

                        bool isSuccess = Convert.ToBoolean(isSuccessParam.Value);
                        int newPaymentId = (newPaymentIdParam.Value != DBNull.Value) ? (int)newPaymentIdParam.Value : 0;

                        if (!isSuccess || newPaymentId <= 0)
                        {
                            transaction.Rollback();  // ❌ Rollback if payment insert failed
                            return false;
                        }

                        // ✅ Insert selected tests
                        if (model.SelectedTestIds != null)
                        {
                            //foreach (var selectedId in model.SelectedTestIds)
                            //{
                            //    using (SqlCommand cmd1 = new SqlCommand("sp_InsertPaymentTest", conn, transaction))
                            //    {
                            //        cmd1.CommandType = CommandType.StoredProcedure;
                            //        cmd1.Parameters.AddWithValue("@PaymentId", newPaymentId);
                            //        cmd1.Parameters.AddWithValue("@TestId", selectedId);

                            //        SqlParameter returnParam = new SqlParameter();
                            //        returnParam.Direction = ParameterDirection.ReturnValue;
                            //        cmd1.Parameters.Add(returnParam);

                            //        await cmd1.ExecuteNonQueryAsync();

                            //        int result = (int)returnParam.Value;
                            //        if (result != 1)
                            //        {
                            //            transaction.Rollback(); // ❌ If one test insert fails, rollback all
                            //            return false;
                            //        }
                            //    }
                            //}

                            foreach (var testId in model.SelectedTestIds)
                            {
                                var cmd2 = new SqlCommand("INSERT INTO PaymentTests (PaymentId, TestId) VALUES (@p,@t); SELECT SCOPE_IDENTITY()", conn, transaction);
                                cmd2.Parameters.AddWithValue("@p", newPaymentId);
                                cmd2.Parameters.AddWithValue("@t", testId);
                                var paymentTestId = Convert.ToInt32(cmd2.ExecuteScalar());

                                // call stored proc using same connection + transaction
                                //using (var sp = new SqlCommand("sp_DeductStock_OnPaymentTest", conn, transaction))
                                //{
                                //    sp.CommandType = CommandType.StoredProcedure;
                                //    sp.Parameters.AddWithValue("@PaymentTestId", paymentTestId);
                                //    sp.ExecuteNonQuery();
                                //}
                            }
                        }

                        transaction.Commit();  // ✅ All good — commit transaction
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // ❌ Rollback on exception
                                            // optionally log error
                    throw;
                }
            }
        }

        public async Task<List<PaymentViewModel>> GetPaymentsByAssistant(int assistantId)
        {
            var list = new List<PaymentViewModel>();

            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("sp_GetPaymentsByAssistant", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AssistantId", assistantId);
            await conn.OpenAsync();
            SqlDataReader reader =await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new PaymentViewModel
                {
                    PatientId = reader["PatientId"] != DBNull.Value ? Convert.ToInt32(reader["PatientId"]) : 0,
                    PaymentId = reader["PaymentId"] != DBNull.Value ? Convert.ToInt32(reader["PaymentId"]) : 0,
                    SampleId = reader["SampleId"] != DBNull.Value ? Convert.ToInt32(reader["SampleId"]) : 0,
                    Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : 0m,
                    PaymentMethod = reader["PaymentMethod"] != DBNull.Value ? reader["PaymentMethod"].ToString() : string.Empty,
                    Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : string.Empty,
                    AssistantId = reader["AssistantId"] != DBNull.Value ? Convert.ToInt32(reader["AssistantId"]) : 0,
                    PaidAmount = reader["PaidAmount"] != DBNull.Value ? Convert.ToDecimal(reader["PaidAmount"]) : 0m,
                    RemaingAmount = reader["RemainingAmount"] != DBNull.Value ? Convert.ToDecimal(reader["RemainingAmount"]) : 0m,
                    AssistantName = reader["AssistantName"] != DBNull.Value ? reader["AssistantName"].ToString() : string.Empty,
                    PatientName = reader["PatientName"] != DBNull.Value ? reader["PatientName"].ToString() : string.Empty,
                    ContactNumber = reader["ContactNumber"] != DBNull.Value ? reader["ContactNumber"].ToString() : string.Empty,
                    Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : string.Empty,
                    CreatedDate = reader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedDate"]) : DateTime.MinValue,
                    PrescriptionImgURL = reader["PrescriptionImgURL"] != DBNull.Value ? reader["PrescriptionImgURL"].ToString() : string.Empty,
                    DiscountAmount = reader["DiscountAmount"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountAmount"]) : 0m
                });
            }
            return list;
        }

        public async Task<PaymentViewModel> GetPaymentsByPaymentId(int paymentId,int assistantId)
        {
            var paymentViewModel = new PaymentViewModel();

            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("sp_GetPaymentById", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PaymentId", paymentId);
            //cmd.Parameters.AddWithValue("@SampleId", paymentId);
            await conn.OpenAsync();
            SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
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
                    DiscountAmount = reader["DiscountAmount"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("DiscountAmount")) : 0m

                };
            }
            return paymentViewModel;
        }

        public async Task<bool> Update(PaymentTransactionViewModel paymentViewModel)
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
                    cmd.Parameters.AddWithValue("@DiscountAmount", paymentViewModel.DiscountAmount);
                    cmd.Parameters.AddWithValue("@RemainingAmount", paymentViewModel.RemaingAmount);

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
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();

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

        public async Task<AssistantDashboardViewModel> GetAssistantDashboardSummary(int assistantId, DateTime? fromDate, DateTime? toDate)
        {
            var model = new AssistantDashboardViewModel();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetAssistantDashboardSummary", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AssistantId", assistantId);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);

                await conn.OpenAsync();
                using (SqlDataReader reader =  await cmd.ExecuteReaderAsync())
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


        public async Task<List<PaymentViewModel>> GetRemainingPaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate)
     =>await GetPayments("sp_GetRemainingPaymentsByAssistant", assistantId, fromDate, toDate);

        public async Task<List<PaymentViewModel>> GetPaidPaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate)
            => await GetPayments("sp_GetPaidPaymentsByAssistant", assistantId, fromDate, toDate);

        public async Task<List<PaymentViewModel>> GetOnlinePaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate)
            => await GetPayments("sp_GetOnlinePaymentsByAssistant", assistantId, fromDate, toDate);

        public async Task<List<PaymentViewModel>> GetCashPaymentsByAssistant(int assistantId, DateTime? fromDate, DateTime? toDate)
            => await GetPayments("sp_GetCashPaymentsByAssistant", assistantId, fromDate, toDate);

        private async Task<List<PaymentViewModel>> GetPayments(string spName, int assistantId, DateTime? fromDate, DateTime? toDate)
        {
            var list = new List<PaymentViewModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(spName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AssistantId", assistantId);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);

                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
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

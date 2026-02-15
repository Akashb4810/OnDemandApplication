using LabCollect.Models;
using LabCollect.Repository.Interface;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Drawing;

namespace LabCollect.Repository.Implementation
{
    public class InventoryService : IInventoryService
    {
        private readonly string _connectionString;
        public InventoryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<int> AddReagent(Reagent r)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand(@"INSERT INTO Reagents (ReagentName, Unit, LowStockLimit) 
                                       VALUES (@n,@u,@a); SELECT SCOPE_IDENTITY();", con);
                cmd.Parameters.AddWithValue("@n", r.Name);
                cmd.Parameters.AddWithValue("@u", (object)r.Unit ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@a", (object)r.LowStockLimit ?? DBNull.Value);
                con.Open();
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }
        //public void UpdateReagent(Reagent r)
        //{
        //    using (var con = new SqlConnection(_connectionString))
        //    {
        //        var cmd = new SqlCommand(@"UPDATE Reagents SET ReagentName=@n, Unit=@u, AlertThreshold=@a WHERE ReagentId=@id", con);
        //        cmd.Parameters.AddWithValue("@id", r.ReagentId);
        //        cmd.Parameters.AddWithValue("@n", r.ReagentName);
        //        cmd.Parameters.AddWithValue("@u", (object)r.Unit ?? DBNull.Value);
        //        cmd.Parameters.AddWithValue("@a", (object)r.AlertThreshold ?? DBNull.Value);
        //        con.Open(); cmd.ExecuteNonQuery();
        //    }
        //}
        public async Task<List<Reagent>> GetAllReagents()
        {
            var list = new List<Reagent>();
            using (var con = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT ReagentId, Name, Unit,LowStockLimit FROM Reagents", con);
                con.Open();
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (r.Read()) list.Add(new Reagent
                    {
                        ReagentId = (int)r["ReagentId"],
                        Name = r["Name"].ToString(),
                        Unit = r["Unit"] as string,
                        LowStockLimit = r["LowStockLimit"] as decimal?
                    });
                }
            }
            return list;
        }

        //// Lots CRUD
        //public async Task<int> AddLot(ReagentLot lot)
        //{
        //    using (var con = new SqlConnection(_connectionString))
        //    {
        //        var cmd = new SqlCommand(@"INSERT INTO ReagentLot (ReagentId,BatchNo,OriginalQuantity,Quantity,ExpiryDate,ReceivedDate)
        //                              VALUES (@rid,@b,@orig,@q,@exp,GETDATE()); SELECT SCOPE_IDENTITY();", con);
        //        cmd.Parameters.AddWithValue("@rid", lot.ReagentId);
        //        cmd.Parameters.AddWithValue("@b", (object)lot.BatchNo ?? DBNull.Value);
        //        cmd.Parameters.AddWithValue("@orig", lot.OriginalQuantity);
        //        cmd.Parameters.AddWithValue("@q", lot.Quantity);
        //        cmd.Parameters.AddWithValue("@exp", (object)lot.ExpiryDate ?? DBNull.Value);
        //        con.Open(); return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        //    }
        //}
        //public async Task<List<ReagentLot>> GetLotsByReagent(int reagentId)
        //{
        //    var list = new List<ReagentLot>();
        //    using (var con = new SqlConnection(_connectionString))
        //    {
        //        var cmd = new SqlCommand(@"SELECT l.*, r.ReagentName FROM ReagentLot l
        //                              JOIN Reagents r ON r.ReagentId = l.ReagentId
        //                              WHERE l.ReagentId = @rid ORDER BY ExpiryDate, ReceivedDate", con);
        //        cmd.Parameters.AddWithValue("@rid", reagentId);
        //        con.Open();
        //        using (var r =await cmd.ExecuteReaderAsync())
        //        {
        //            while (r.Read()) list.Add(new ReagentLot
        //            {
        //                LotId = (int)r["LotId"],
        //                ReagentId = (int)r["ReagentId"],
        //                BatchNo = r["BatchNo"] as string,
        //                OriginalQuantity = (decimal)r["OriginalQuantity"],
        //                Quantity = (decimal)r["Quantity"],
        //                ExpiryDate = r["ExpiryDate"] as DateTime?,
        //                ReceivedDate = (DateTime)r["ReceivedDate"],
        //                ReagentName = r["ReagentName"].ToString()
        //            });
        //        }
        //    }
        //    return list;
        //}

        //// TestReagentUsage CRUD
        public async Task<int> AddOrUpdateTestReagentUsage(AddTestReagentUsageModel model)
        {
            int rowsAffected = 0;

            using (var con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                // Get already mapped reagents for this test
                var existing = await GetAllTestReagents(model.TestId);

                foreach (var item in model.Reagents)
                {
                    var exists = existing.FirstOrDefault(x => x.ReagentId == item.ReagentId);

                    if (exists != null)
                    {
                        // UPDATE existing mapping
                        var updateCmd = new SqlCommand(@"
                    UPDATE TestReagentUsage
                    SET QuantityPerTest = @Qty
                    WHERE TestId = @TestId AND ReagentId = @ReagentId", con);

                        updateCmd.Parameters.AddWithValue("@TestId", model.TestId);
                        updateCmd.Parameters.AddWithValue("@ReagentId", item.ReagentId);
                        updateCmd.Parameters.AddWithValue("@Qty", item.QuantityPerTest);

                        rowsAffected += await updateCmd.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        // INSERT new mapping
                        var insertCmd = new SqlCommand(@"
                    INSERT INTO TestReagentUsage (TestId, ReagentId, QuantityPerTest)
                    VALUES (@TestId, @ReagentId, @Qty)", con);

                        insertCmd.Parameters.AddWithValue("@TestId", model.TestId);
                        insertCmd.Parameters.AddWithValue("@ReagentId", item.ReagentId);
                        insertCmd.Parameters.AddWithValue("@Qty", item.QuantityPerTest);

                        rowsAffected += await insertCmd.ExecuteNonQueryAsync();
                    }
                }
            }

            return rowsAffected;
        }


        public async Task<List<TestReagentUsageModel>> GetUsageByTest(int testId)
        {
            var list = new List<TestReagentUsageModel>();
            using (var con = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand(@"SELECT u.UsageId,u.TestId,u.ReagentId,u.QuantityPerTest,r.ReagentName,lt.TestName
                                      FROM TestReagentUsage u
                                      JOIN Reagents r ON r.ReagentId = u.ReagentId
                                      LEFT JOIN LaboratoryTests lt ON lt.TestId = u.TestId
                                      WHERE u.TestId = @t", con);
                cmd.Parameters.AddWithValue("@t", testId);
                con.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read()) list.Add(new TestReagentUsageModel
                    {
                        UsageId = (int)rd["UsageId"],
                        TestId = (int)rd["TestId"],
                        ReagentId = (int)rd["ReagentId"],
                        QuantityPerTest = (decimal)rd["QuantityPerTest"],
                        ReagentName = rd["ReagentName"].ToString(),
                        TestName = rd["TestName"] as string
                    });
                }
            }
            return list;
        }

        //// Stock capacity (call SP)
        //public async Task<DataTable> GetTestCapacity(int testId)
        //{
        //    using (var con = new SqlConnection(_connectionString))
        //    {
        //        var cmd = new SqlCommand("sp_GetTestCapacity", con) { CommandType = CommandType.StoredProcedure };
        //        cmd.Parameters.AddWithValue("@TestId", testId);
        //        var dt = new DataTable();
        //        using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
        //        return dt;
        //    }
        //}
        //public async Task<int> GetFinalTestCapacity(int testId)
        //{
        //    using (var con = new SqlConnection(_connectionString))
        //    {
        //        var cmd = new SqlCommand("sp_GetFinalTestCapacity", con) { CommandType = CommandType.StoredProcedure };
        //        cmd.Parameters.AddWithValue("@TestId", testId);
        //        con.Open();
        //        var obj =await cmd.ExecuteScalarAsync();
        //        return obj == DBNull.Value || obj == null ? 0 : Convert.ToInt32(obj);
        //    }
        //}

        //// Call deduct SP
        //public void DeductStockForPaymentTest(int paymentTestId)
        //{
        //    using (var con = new SqlConnection(_connectionString))
        //    {
        //        var cmd = new SqlCommand("sp_DeductStock_OnPaymentTest", con) { CommandType = CommandType.StoredProcedure };
        //        cmd.Parameters.AddWithValue("@PaymentTestId", paymentTestId);
        //        con.Open(); cmd.ExecuteNonQuery();
        //    }
        //}

        public List<ReagentIndexViewModel> GetReagentIndex()
        {
            var list = new List<ReagentIndexViewModel>();
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("sp_GetReagentIndexData", con) { CommandType = CommandType.StoredProcedure })
            {
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ReagentIndexViewModel
                        {
                            ReagentId = (int)r["ReagentId"],
                            Name = r["Name"].ToString(),
                            Unit = r["Unit"] as string,
                            TotalQty = Convert.ToDecimal(r["TotalQty"]),
                           // TotalTestsDone = Convert.ToInt32(r["TotalTestsDone"]),
                            LowStockLimit = Convert.ToDecimal(r["LowStockLimit"])
                        });
                    }
                }
            }
            return list;
        }

        public List<ReagentLot> GetLotsByReagent(int reagentId)
        {
            var list = new List<ReagentLot>();
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("sp_GetLotsByReagent", con) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@ReagentId", reagentId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ReagentLot
                        {
                            LotId = (int)r["LotId"],
                            ReagentId = (int)r["ReagentId"],
                            LotNumber = r["LotNumber"] as string,
                            //OriginalQuantity = Convert.ToDecimal(r["OriginalQuantity"]),
                            Quantity = Convert.ToDecimal(r["Quantity"]),
                            TestsPerUnit = Convert.ToInt32(r["TestsPerUnit"]),
                            //TestsDone = Convert.ToInt32(r["TestsDone"]),
                            //ExpiryDate = r["ExpiryDate"] as DateTime?,
                            //ReceivedDate = Convert.ToDateTime(r["ReceivedDate"])
                        });
                    }
                }
            }
            return list;
        }

        public ReagentLot GetLotById(int lotId)
        {
           
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("sp_GetLotById", con) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@LotId", lotId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        return new ReagentLot
                        {
                            LotId = (int)r["LotId"],
                            ReagentId = (int)r["ReagentId"],
                            LotNumber = r["LotNumber"] as string,
                            //OriginalQuantity = Convert.ToDecimal(r["OriginalQuantity"]),
                            Quantity = Convert.ToDecimal(r["Quantity"]),
                            TestsPerUnit = Convert.ToInt32(r["TestsPerUnit"]),
                            TestsDone = Convert.ToInt32(r["TestsDone"]),
                            //ExpiryDate = r["ExpiryDate"] as DateTime?,
                            //ReceivedDate = Convert.ToDateTime(r["ReceivedDate"])
                        };
                    }
                }
            }
            return null;
        }

        public void AddLot(ReagentLot model)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("sp_AddReagentLot", con) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@ReagentId", model.ReagentId);
                cmd.Parameters.AddWithValue("@LotNumber", (object)model.LotNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OriginalQuantity", model.OriginalQuantity);
                cmd.Parameters.AddWithValue("@ExpiryDate", (object)model.ExpiryDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TestsPerUnit", model.TestsPerUnit);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateTestsDone(int lotId, int completedTests)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("sp_UpdateTestsDone", con) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@LotId", lotId);
                cmd.Parameters.AddWithValue("@CompletedTests", completedTests);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteLot(int lotId)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("DELETE FROM ReagentLot WHERE LotId = @LotId", con))
            {
                cmd.Parameters.AddWithValue("@LotId", lotId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Deduct stock by calling sp_DeductStock_OnPaymentTest
        public void DeductStockForPaymentTest(int paymentTestId)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("sp_DeductStock_OnPaymentTest", con) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@PaymentTestId", paymentTestId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Report
        public DataTable GetTestsDonePerLot()
        {
            var dt = new DataTable();
            using (var con = new SqlConnection(_connectionString))
            using (var da = new SqlDataAdapter("sp_GetTestsDonePerLot", con))
            {
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.Fill(dt);
            }
            return dt;
        }

        public async Task<List<TestReagentUsageViewModel>> GetAllTestReagents(int testId)
        {
            var list = new List<TestReagentUsageViewModel>();

            using (var con = new SqlConnection(_connectionString))
            {
                var query = @"
            SELECT 
                tu.UsageId,
                tu.TestId,
                lt.TestName,
                tu.ReagentId,
                r.Name AS ReagentName,
                tu.QuantityPerTest,
                r.Unit,
                r.LowStockLimit
            FROM TestReagentUsage tu
            JOIN Reagents r ON tu.ReagentId = r.ReagentId
            JOIN LaboratoryTests lt ON tu.TestId = lt.TestId
            WHERE tu.TestId = @TestId
        ";

                var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@TestId", testId);

                await con.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new TestReagentUsageViewModel
                        {
                            UsageId = reader.GetInt32(reader.GetOrdinal("UsageId")),
                            TestId = reader.GetInt32(reader.GetOrdinal("TestId")),
                            TestName = reader["TestName"].ToString(),
                            ReagentId = reader.GetInt32(reader.GetOrdinal("ReagentId")),
                            ReagentName = reader["ReagentName"].ToString(),
                            QuantityPerTest = reader.GetDecimal(reader.GetOrdinal("QuantityPerTest")),
                            Unit = reader["Unit"]?.ToString(),
                            LowStockLimit = reader.GetDecimal(reader.GetOrdinal("LowStockLimit"))
                        });
                    }
                }
            }

            return list;
        }

        public async Task<int> InsertLot(ReagentsLot model)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = @"
            INSERT INTO ReagentLot (LotNumber, ReceivedDate)
            VALUES (@LotNumber, @ReceivedDate);
            SELECT SCOPE_IDENTITY();";

                cmd.Parameters.AddWithValue("@LotNumber", model.LotNumber);
                cmd.Parameters.AddWithValue("@ReceivedDate", model.ReceivedDate);

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task AddLotDetail(ReagentLotDetail detail)
        {
            try { 
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.CommandText = @"
            INSERT INTO ReagentLotDetail
            (LotId, ReagentId, Quantity, OriginalQuantity, TestsPerUnit, ExpiryDate)
            VALUES
            (@LotId, @ReagentId, @Quantity, @OriginalQuantity, @TestsPerUnit, @ExpiryDate)";

                cmd.Parameters.AddWithValue("@LotId", detail.LotId);
                cmd.Parameters.AddWithValue("@ReagentId", detail.ReagentId);
                cmd.Parameters.AddWithValue("@Quantity", detail.Quantity);
                cmd.Parameters.AddWithValue("@OriginalQuantity", detail.OriginalQuantity);
                cmd.Parameters.AddWithValue("@TestsPerUnit", detail.TestsPerUnit);
                cmd.Parameters.AddWithValue("@ExpiryDate", detail.ExpiryDate);

                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
            }catch(Exception ex)
            { throw ex; }
        }


    }
}

using System.Data;
using LabCollect.Models;
using LabCollect.Repository.Interface;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace LabCollect.Repository.Implementation
{
    public class TestService:ITestsService
    {
        private readonly string _connectionString;

        public TestService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> InsertAsync(LaboratoryTestModel model)
        {
            return await ExecuteNonQueryAsync("I", model);
        }

        // 🟡 UPDATE
        public async Task<bool> UpdateAsync(LaboratoryTestModel model)
        {
            return await ExecuteNonQueryAsync("U", model);
        }

        // 🔴 DELETE
        public async Task<bool> DeleteAsync(int testId)
        {
            var model = new LaboratoryTestModel { TestId = testId };
            return await ExecuteNonQueryAsync("D", model);
        }

        // 📋 SELECT ALL
        public async Task<List<LaboratoryTestModel>> GetAllAsync()
        {
            var list = new List<LaboratoryTestModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_LaboratoryTests_IUD", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "S");

                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new LaboratoryTestModel
                        {
                            TestId = Convert.ToInt32(reader["TestId"]),
                            TestName = reader["TestName"].ToString(),
                            Description = reader["Description"]?.ToString(),
                            CategoryId = reader["CategoryId"] != DBNull.Value ? Convert.ToInt32(reader["CategoryId"]) : 0,
                            CategoryName = reader["CategoryName"]?.ToString(),
                            NormalRange = reader["NormalRange"]?.ToString(),
                            Unit = reader["Unit"]?.ToString(),
                            Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0,
                            CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                            UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["UpdatedDate"])
                        });
                    }
                }
            }

            return list;
        }

        // 🔍 GET BY ID
        public async Task<LaboratoryTestModel> GetByIdAsync(int? testId)
        {
            LaboratoryTestModel model = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_LaboratoryTests_IUD", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "G");
                cmd.Parameters.AddWithValue("@TestId", testId);

                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        model = new LaboratoryTestModel
                        {
                            TestId = Convert.ToInt32(reader["TestId"]),
                            TestName = reader["TestName"].ToString(),
                            Description = reader["Description"]?.ToString(),
                            CategoryId = reader["CategoryId"] != DBNull.Value ? Convert.ToInt32(reader["CategoryId"]) : 0,
                            CategoryName = reader["CategoryName"]?.ToString(),
                            NormalRange = reader["NormalRange"]?.ToString(),
                            Unit = reader["Unit"]?.ToString(),
                            Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0,
                            CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                            UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["UpdatedDate"])
                        };
                    }
                }
            }

            return model;
        }

        // ⚡ Common NonQuery Handler (Insert/Update/Delete)
        private async Task<bool> ExecuteNonQueryAsync(string flag, LaboratoryTestModel model)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_LaboratoryTests_IUD", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", flag);
                    cmd.Parameters.AddWithValue("@TestId", model.TestId);
                    cmd.Parameters.AddWithValue("@TestName", (object?)model.TestName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", (object?)model.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryId", (object?)model.CategoryId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NormalRange", (object?)model.NormalRange ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Unit", (object?)model.Unit ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Price", (object?)model.Price ?? DBNull.Value);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log error here if needed
                Console.WriteLine($"[ERROR] {ex.Message}");
                return false;
            }
        }


        public async Task<List<SelectListItem>> GetCategories()
        {
            var categories = new List<SelectListItem>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT CategoryId, CategoryName FROM TestCategory", conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new SelectListItem
                        {
                            Value = reader["CategoryId"].ToString(),
                            Text = reader["CategoryName"].ToString()
                        });
                    }
                }
            }

            return categories;
        }
    }
}

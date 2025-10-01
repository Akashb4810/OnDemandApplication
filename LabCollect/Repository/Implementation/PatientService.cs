using LabCollect.Models;
using LabCollect.Repository.Interface;
using Microsoft.Data.SqlClient;

namespace LabCollect.Repository.Implementation
{
    public class PatientService:IPatientService
    {
        private readonly string _connectionString;

        public PatientService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public List<Patient> SearchPatientsByName(string name)
        {
            var list = new List<Patient>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 10 * FROM Patients WHERE PatientName LIKE @name + '%'", conn))
            {
                cmd.Parameters.AddWithValue("@name", name);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Patient
                        {
                            PatientId = Convert.ToInt32(reader["PatientId"]),
                            PatientName = reader["PatientName"].ToString(),
                            ContactNumber = reader["ContactNumber"].ToString(),
                            Email = reader["Email"].ToString(),
                            Address = reader["Address"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        public Patient GetPatientByName(string name)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM Patients WHERE PatientName=@name", conn))
            {
                cmd.Parameters.AddWithValue("@name", name);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Patient
                        {
                            PatientId = Convert.ToInt32(reader["PatientId"]),
                            PatientName = reader["PatientName"].ToString(),
                            ContactNumber = reader["ContactNumber"].ToString(),
                            Email = reader["Email"].ToString(),
                            Address = reader["Address"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        public Patient GetPatientById(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM Patients WHERE PatientId=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Patient
                        {
                            PatientId = Convert.ToInt32(reader["PatientId"]),
                            //SampleId = reader["SampleId"] != DBNull.Value ? Convert.ToInt32(reader["SampleId"]) : 0,
                            PatientName = reader["PatientName"].ToString(),

                            // Handle possible NULLs:
                            DateOfBirth = reader["DateOfBirth"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["DateOfBirth"]),

                            Gender = reader["Gender"] == DBNull.Value
                                ? null
                                : reader["Gender"].ToString(),

                            ContactNumber = reader["ContactNumber"] == DBNull.Value
                                ? null
                                : reader["ContactNumber"].ToString(),

                            Email = reader["Email"] == DBNull.Value
                                ? null
                                : reader["Email"].ToString(),

                            Address = reader["Address"] == DBNull.Value
                                ? null
                                : reader["Address"].ToString()                          
                        };
                    }
                }
            }
            return null;
        }

    }
}

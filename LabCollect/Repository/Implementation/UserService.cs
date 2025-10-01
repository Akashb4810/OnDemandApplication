using System.Data;
using System.Security.Cryptography;
using System.Text;
using LabCollect.Models;
using LabCollect.Repository.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace LabCollect.Repository.Implementation
{
    public class UserService : IUserService
    {
        private readonly string _connectionString;
        private readonly IPasswordHasher<UserLoginResult> _passwordHasher;
        public UserService(IConfiguration configuration, IPasswordHasher<UserLoginResult> passwordHasher)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _passwordHasher = passwordHasher;
        }


        public List<AppTypeViewModel> GetAppTypes()
        {
            var list = new List<AppTypeViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAppTypes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        list.Add(new AppTypeViewModel
                        {
                            AppTypeId = reader.GetInt32(0),
                            AppTypeName = reader.GetString(1)
                        });
                    }
                }
            }

            return list;

        }

        public UserLoginResult GetUserByCredentials(string username, string password, int appTypeId)
        {
            UserLoginResult user = null;
            string hashed = HashPassword(password);
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetUserByCredentials", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", username);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashed);
                    // We’ll pass AppTypeName or ID; adjust your SP accordingly
                    cmd.Parameters.AddWithValue("@AppTypeName", GetAppTypeNameById(appTypeId));

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        user = new UserLoginResult
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            UserName = reader.GetString(reader.GetOrdinal("UserName")),
                            RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                            AppTypeName = reader.GetString(reader.GetOrdinal("AppTypeName"))
                        };
                    }
                }
            }

            return user;
        }
        private string GetAppTypeNameById(int appTypeId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT AppTypeName FROM AppTypes WHERE AppTypeId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", appTypeId);
                    conn.Open();
                    return cmd.ExecuteScalar()?.ToString();
                }
            }
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public UserLoginResult ValidateUser(string username, string password, int appTypeId)
        {
            UserLoginResult? user = null;
            string? passwordHashFromDb = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetUserByUsernameAppType", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserName", username);
                cmd.Parameters.AddWithValue("@AppTypeId", appTypeId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {

                    passwordHashFromDb = reader.GetString(reader.GetOrdinal("PasswordHash"));

                    user = new UserLoginResult
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        UserName = reader.GetString(reader.GetOrdinal("UserName")),
                        RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                        AppTypeName = reader.GetString(reader.GetOrdinal("AppTypeName"))
                    };
                }
            }

            if (user == null || passwordHashFromDb == null)
                return null;

            string hashedInput = HashPassword(password);  // your SHA256 hash method
            if (!string.Equals(hashedInput, passwordHashFromDb, StringComparison.OrdinalIgnoreCase))
                return null;

            return user;
        }
    }
}


using DAL.Interfaces;
using DAL.Models;
using DAL.Models.User;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DAL.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _configurationSystem;
        public string _connectionString = null;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserRepository(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configurationSystem = configuration;
            _connectionString = _configurationSystem.GetConnectionString("MyCon");
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<AuthResponse> Login(string username, string password)
        {
            try
            {
                Users user = await GetUserByUserName(username);
                if (user == null || user.ActiveFlag != false || username != user.UserName)
                    return null;
                bool isValid = CheckPassword(password, user.Password);

                if (!isValid)
                    return null;

                var authResponse = await GenerateTokenResponse(user);
                return authResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private async Task<AuthResponse> GenerateTokenResponse(Users user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configurationSystem["Jwt:SecretKey"]);
            var connect = await GetConnetionByHospitalId(user.HOSPITALID);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.USERID.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Emailid),
                new Claim(ClaimTypes.Role, user.RoleId.ToString()),
                new Claim(ClaimTypes.Role, user.RoleName.ToString()),
                new Claim("MboilePhone", user.MobileNumber.ToString()),
                new Claim("HospitalId", user.HOSPITALID.ToString()),
                new Claim("Connection", connect.ToString())
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configurationSystem["Jwt:AccessTokenExpirationMinutes"])),
                Issuer = _configurationSystem["Jwt:Issuer"],
                Audience = _configurationSystem["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponse
            {
                Token = tokenHandler.WriteToken(token),
                CreatedOn = DateTime.UtcNow,
                Expires = tokenDescriptor.Expires,
            };
        }


        public async Task<Users> GetUserByUserName(string username)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserName", username, DbType.String);

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                try
                {
                    await con.OpenAsync();

                    var user = await con.QuerySingleOrDefaultAsync<Users>(
                        "",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return user;
                }
                catch (Exception ex)
                {
                    // Log or handle exception
                    throw;
                }
            }
        }


        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes);
            }
        }
        private bool CheckPassword(string enteredPassword, string storedHash)
        {
            string hashedEnteredPassword = HashPassword(enteredPassword);

            return hashedEnteredPassword == storedHash;
        }

        public async Task<string> GetConnetionByHospitalId(Guid hospitalId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@hospital_id", hospitalId.ToString(), DbType.String);

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                try
                {
                    await con.OpenAsync();

                    var user = await con.QuerySingleOrDefaultAsync<string>("", parameters, commandType: CommandType.StoredProcedure
                    );

                    return user;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<LoginResponse> GetDataFromToken(HttpContext context)
        {
            try
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();

                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    throw new Exception("Invalid or missing Authorization header.");
                }

                var tokenStr = authHeader.Substring("Bearer ".Length).Trim();
                var tokenHandler = new JwtSecurityTokenHandler();

                var token = tokenHandler.ReadJwtToken(tokenStr);
                var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                var uniqueNameClaim = token.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
                var emailClaim = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                var mobileClaim = token.Claims.FirstOrDefault(c => c.Type == "MobilePhone")?.Value;
                var hospitalIdClaim = token.Claims.FirstOrDefault(c => c.Type == "HospitalId")?.Value;
                var connectionClaim = token.Claims.FirstOrDefault(c => c.Type == "Connection")?.Value;
                var roleCliam = token.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

                if (userIdClaim == null || uniqueNameClaim == null || emailClaim == null)
                {
                    throw new Exception("Required claims are missing.");
                }

                var loginResponse = new LoginResponse
                {
                    USERID = Guid.Parse(userIdClaim),
                    Emailid = emailClaim,
                    Connection = connectionClaim,
                    HOSPITALID = hospitalIdClaim,
                    MobileNumber = string.IsNullOrWhiteSpace(mobileClaim) ? (long?)null : long.Parse(mobileClaim) ,
                    RoleId = Guid.Parse(roleCliam)
                };

                    return loginResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while processing token: " + ex.Message);
            }
        }

        public async Task<LoginResponse> GetSessionData(HttpContext context)
        {
            try
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();

                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    throw new Exception("Invalid or missing Authorization header.");
                }

                var tokenStr = authHeader.Substring("Bearer ".Length).Trim();
                var tokenHandler = new JwtSecurityTokenHandler();

                var token = tokenHandler.ReadJwtToken(tokenStr);

                var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                var uniqueNameClaim = token.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
                var emailClaim = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                var mobileClaim = token.Claims.FirstOrDefault(c => c.Type == "MobilePhone")?.Value;
                var hospitalIdClaim = token.Claims.FirstOrDefault(c => c.Type == "HospitalId")?.Value;
                var connectionClaim = token.Claims.FirstOrDefault(c => c.Type == "Connection")?.Value;
                var roleCliam = token.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

                // Validate extracted claims
                //if (userIdClaim == null || uniqueNameClaim == null || emailClaim == null)
                //{

                //    throw new Exception("Required claims are missing.");

                //    LoginResponse loginResponse1 = new LoginResponse()
                //    {

                //       // Connection = "Data Source=SQL1003.site4now.net;Initial Catalog=db_ab79a1_antiquedev;User Id=db_ab79a1_antiquedev_admin;Password=antiqueDev@25;"
                //        Connection = "Data Source=sql1003.site4now.net;Initial Catalog=db_ab79a1_antiquedev;User Id=db_ab79a1_antiquedev_admin;Password=antiqueDev@25;",

                //        USERID = Guid.Parse("615CFB1F-EC0E-483D-8256-C782B38F23BC"),
                //        HOSPITALID = "15C683E0-4FD5-4358-BEDC-C0E2D33F9747",
                //        MobileNumber = 9607345814,
                //        Emailid = "adityasawant5814@gmail.com"
                //    };
                //    return loginResponse1;

                //}

                Guid userId;
                if (!Guid.TryParse(userIdClaim, out userId))
                {
                    throw new Exception("Invalid 'nameid' format.");
                }

                var loginResponse = new LoginResponse
                {
                    USERID = userId,
                    Emailid = emailClaim,
                    Connection = connectionClaim,
                    HOSPITALID = hospitalIdClaim,
                    MobileNumber = string.IsNullOrWhiteSpace(mobileClaim) ? (long?)null : long.Parse(mobileClaim),
                    RoleId = Guid.Parse(roleCliam)
                };

                return loginResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while processing token: " + ex.Message);
            }
        }

        public Task<string> GetConnectionByContext(HttpContext httpContext)
        {
            var hospitalIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "HospitalId");
            if (hospitalIdClaim == null || !Guid.TryParse(hospitalIdClaim.Value, out Guid hospitalId))
            {
                throw new UnauthorizedAccessException("HospitalId claim is missing or invalid.");
            }
            var Connectionstring = GetConnetionByHospitalId(hospitalId);
            return Connectionstring;
        }
    }
}

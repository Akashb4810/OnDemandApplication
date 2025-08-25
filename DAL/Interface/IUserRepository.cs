using DAL.Models.User;
using Microsoft.AspNetCore.Http;

namespace DAL.Interfaces
{
    public interface IUserRepository
    {
        //Task<LoginResponse> Login(string username, string password);
        Task<AuthResponse> Login(string username, string password);
        Task<LoginResponse> GetDataFromToken(HttpContext context);
        Task<LoginResponse> GetSessionData(HttpContext context);
        Task<string>GetConnectionByContext(HttpContext httpContext);
    }
}

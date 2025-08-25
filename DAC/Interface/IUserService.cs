using DAL.Models.User;
using Microsoft.AspNetCore.Http;

namespace DAC.Interfaces
{
    public interface IUserService
    {
        //Task<LoginResponse> Login(string username, string password);
        Task<AuthResponse> Login(string username, string password);
        Task<dynamic> GetDataFromToken(HttpContext context);
    }
}                                           

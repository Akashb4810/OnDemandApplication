using DAC.Interfaces;
using DAL.Interfaces;
using DAL.Models.User;
using Microsoft.AspNetCore.Http;

namespace DAC.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<dynamic> GetDataFromToken(HttpContext context)
        {
            return await _userRepository.GetDataFromToken(context);
        }

        public async Task<AuthResponse> Login(string username, string password)
        {
            var loginResponse = await _userRepository.Login(username, password);
            //if (loginResponse != null)
            //{
            //    await _emailRepository.SendOtpEmail("1234");
            //}

            return loginResponse;
        }
    }
}

using Azure;
using DAC.Implementations;
using DAC.Interfaces;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HIMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {   
        private readonly IUserService _userServices;
       // private readonly ILogExceptionRepository _logException;

        public UserController(IUserService userServices/*, ILogExceptionRepository logException*/)
        {
            _userServices = userServices;
            //_logException = logException;
        }

        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn(string username, string password,IUserService? userService)
        {
            try
            {
                var response = await _userServices.Login(username, password);

                if (response == null)
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Invalid Username or Password",
                        Data = (object)null
                    });
                }

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "User Logged In Successfully",
                    Data = response
                });

            }
            catch (Exception ex)
            {
                //await _logException.InsertLog(ex.Message, DateTime.Now, "Error", 
                //    ControllerContext.ActionDescriptor.ControllerName,
                //    ControllerContext.ActionDescriptor.ActionName
                //);

                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An unexpected error occurred.",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("TokenRespons")]
        public async Task<IActionResult> TokenRespons()
        {
            var tokenData = await _userServices.GetDataFromToken(HttpContext);
            if(tokenData == null)
            {
                return BadRequest(new{StatusCode = 400,Message = "Invalid Username or Password",Data = (object)null});
            }
            return Ok(new
            { StatusCode = 200,Message = "User Logged In Successfully",Data = tokenData });
        }

    }
}

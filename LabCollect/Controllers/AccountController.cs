using System.Security.Claims;
using LabCollect.Models;
using LabCollect.Repository.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace LabCollect.Controllers
{
    public class AccountController : Controller
    {
      // private readonly IAccountService _accountService;
       private readonly IUserService _userService;
      
        public AccountController(/*IAccountService accountService,*/ IUserService userService)
        {
           //_accountService = accountService;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Load AppTypes dynamically from DB
            var model = new LoginViewModel();
            model.AppTypes = _userService.GetAppTypes();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // <-- CSRF protection
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AppTypes = _userService.GetAppTypes();
                return View(model);
            }

            // 1. Check credentials (hashed password verification in service)
            var user = _userService.ValidateUser(model.UserName, model.Password, model.SelectedAppType);

            if (user == null)
            {
                ViewBag.Error = "Invalid username/password or app type.";
                model.AppTypes = _userService.GetAppTypes();
                return View(model);
            }

            // 2. Build claims identity instead of raw session
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Role, user.RoleName),
        new Claim("AppTypeName", user.AppTypeName)
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
               // IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // 3. Redirect by role/app type
            if (user.AppTypeName == "Lab")
            {
                if (user.RoleName == "Owner")
                    return RedirectToAction("Index", "LabOwnerDashboard");
                else if (user.RoleName == "Assistant")
                    return RedirectToAction("Index", "Assistant");
            }
            else if (user.AppTypeName == "Dairy")
            {
                return RedirectToAction("Index", "DairyDashboard");
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            // Sign out the authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect to login page
            return RedirectToAction("Login", "Account");
        }
    }
}

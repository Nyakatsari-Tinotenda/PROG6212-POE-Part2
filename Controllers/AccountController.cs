using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using POE_Project.Models;

namespace POE_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ViewBag.Error = "Invalid credentials.";
                return View(model);
            }

            // Check if selected department matches the role
            if (!await _userManager.IsInRoleAsync(user, model.Department))
            {
                ViewBag.Error = "You are not assigned to this department.";
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

            if (!result.Succeeded)
            {
                ViewBag.Error = "Invalid login attempt.";
                return View(model);
            }

            // Redirect based on role
            return model.Department switch
            {
                "Lecturer" => RedirectToAction("Lecturer", "Home"),
                "Coordinator" => RedirectToAction("Coordinator", "Home"),
                "Manager" => RedirectToAction("Manager", "Home"),
                "HR" => RedirectToAction("HR", "Home"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}

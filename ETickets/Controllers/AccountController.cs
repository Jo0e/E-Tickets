using ETickets.Models;
using ETickets.Utility;
using ETickets.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ETickets.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AccountController(UserManager<ApplicationUser> userManager , SignInManager<ApplicationUser> signInManager,RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }

        public async Task<IActionResult> Register()
        {
            if (roleManager.Roles.IsNullOrEmpty())
            {   
                await roleManager.CreateAsync(new IdentityRole(SD.AdminRole));
                await roleManager.CreateAsync(new IdentityRole(SD.CustomerRole));
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(ApplicationUserVM userVM)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser applicationUser = new ApplicationUser()
                {
                    UserName =userVM.Username,
                    Email =userVM.Email,
                    City =userVM.City,

                };
                var result = await userManager.CreateAsync(applicationUser, userVM.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(applicationUser, SD.CustomerRole);
                    await signInManager.SignInAsync(applicationUser, isPersistent: false); 
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("Password", $"Invalid Password : - requires contain an uppercase character, - lowercase character, - a digit, and - a non-alphanumeric character. - Passwords must be at least eight characters long.");
            }
            return View(userVM);
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM userVM)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(userVM.Username);
                if (user != null) 
                {
                 var finalResult =  await userManager.CheckPasswordAsync(user, userVM.Password);

                    if (finalResult)
                    {
                        await signInManager.SignInAsync(user, userVM.RememberMe);
                        return RedirectToAction("Index", "Home");
                    }
                    else 
                    {
                        ModelState.AddModelError("Password", "Invalid Password");
                    }
                }
                ModelState.AddModelError("Username", "Invalid Username");
                
            }
            return View(userVM);
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return View("Login");
        }



    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Cumulus_Flights.Models.Enums;
using FBMS_CumulusFlights.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FBMS_CumulusFlights.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username or Email")]
            public string UserNameOrEmail { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                // First try to find by username
                var user = await _userManager.FindByNameAsync(Input.UserNameOrEmail);

                // If not found by username, try email
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(Input.UserNameOrEmail);
                }

                if (user != null)
                {
                    // Use the actual username from the user object
                    var result = await _signInManager.PasswordSignInAsync(
                        user.UserName, // This MUST match the stored username
                        Input.Password,
                        Input.RememberMe,
                        lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        // Add UserType to claims
                        var claims = new List<Claim>
                {
                    new Claim("UserType", user.UserType.ToString())
                };

                        // Sign in with the additional claims
                        await _signInManager.SignInWithClaimsAsync(user, Input.RememberMe, claims);

                        // Redirect based on UserType
                        if (user.UserType == UserTypeEnum.Admin || user.UserType == UserTypeEnum.SuperAdmin)
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        else
                        {
                            return LocalRedirect(ReturnUrl);
                        }
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return Page();
        }
    }
}
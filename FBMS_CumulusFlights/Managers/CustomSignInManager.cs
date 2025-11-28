using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using FBMS_CumulusFlights.Models;
using FBMS_CumulusFlights.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace FBMS_CumulusFlights.Managers
{
    public class CustomSignInManager : SignInManager<ApplicationUser>
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomSignInManager(
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<ApplicationUser> confirmation,
            ApplicationDbContext context)
            : base(userManager, httpContextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var result = await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);

            if (result.Succeeded)
            {
                var user = await UserManager.FindByNameAsync(userName);
                if (user != null)
                {
                    // Add UserType claim
                    var claims = await UserManager.GetClaimsAsync(user);
                    var userTypeClaim = claims.FirstOrDefault(c => c.Type == "UserType");

                    if (userTypeClaim == null)
                    {
                        await UserManager.AddClaimAsync(user, new Claim("UserType", user.UserType.ToString()));
                    }

                    // Log the login activity
                    var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                    _context.UserActivityLogs.Add(new UserActivityLog
                    {
                        UserId = user.Id,
                        LoginTime = DateTime.UtcNow,
                        IpAddress = ipAddress,
                        IsSuccessful = true
                    });

                    await _context.SaveChangesAsync();
                }
            }

            return result;
        }

        public override async Task SignOutAsync()
        {
            var user = await UserManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (user != null)
            {
                var log = await _context.UserActivityLogs
                    .OrderByDescending(l => l.LoginTime)
                    .FirstOrDefaultAsync(l => l.UserId == user.Id && l.LogoutTime == null);

                if (log != null)
                {
                    log.LogoutTime = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            await base.SignOutAsync();
        }
    }
}

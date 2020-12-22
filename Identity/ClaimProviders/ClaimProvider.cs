using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Identity.ClaimProviders
{

    public class ClaimProvider : IClaimsTransformation
    {
        private readonly UserManager<AppUser> _userManager;

        public ClaimProvider(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // principal, controller içinde kullandığımız User ile aynı
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal != null && principal.Identity.IsAuthenticated)
            {
                // User.Identity
                var identity = principal.Identity as ClaimsIdentity;
                var user = await _userManager.FindByNameAsync(identity.Name);
                if (user != null)
                {
                    if (user.BirthDate != null)
                    {
                        var age = DateTime.UtcNow.Year - user.BirthDate?.Year;
                        if (age > 15)
                            identity.AddClaim(new Claim("violence", "true", ClaimValueTypes.Boolean, "Internal"));
                    }

                    if (user.City != null && !principal.HasClaim(c => c.Type == "city"))
                        identity.AddClaim(new Claim("city", user.City, ClaimValueTypes.String, "Internal"));
                }
            }

            return principal;
        }
    }
}
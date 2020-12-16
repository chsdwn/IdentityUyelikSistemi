using System.Collections.Generic;
using System.Threading.Tasks;
using Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.CustomValidation
{
    public class CustomUserValidator : IUserValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            var errors = new List<IdentityError>();
            var numericCharacter = new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            foreach (var numericChar in numericCharacter)
                if (user.UserName.StartsWith(numericChar))
                    errors.Add(new IdentityError
                    {
                        Code = "UsernameStartsWithANumericCharacter",
                        Description = "Kullanıcı adı sayı ile başlayamaz"
                    });

            if (errors.Count == 0)
                return Task.FromResult(IdentityResult.Success);
            else
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
        }
    }
}
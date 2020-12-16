using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.CustomValidation
{
    public class CustomPasswordValidator : IPasswordValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(
            UserManager<AppUser> manager,
            AppUser user,
            string password)
        {
            var errors = new List<IdentityError>();

            if (password.ToLower().Contains(user.UserName.ToLower()))
                errors.Add(new IdentityError
                {
                    Code = "PasswordContainsUsername",
                    Description = "Şifre kullanıcı adı içeremez"
                });

            if (password.ToLower().Contains(user.Email.ToLower()))
                errors.Add(new IdentityError
                {
                    Code = "PasswordContainsEmail",
                    Description = "Şifre email içeremez"
                });

            if (errors.Count == 0)
                return Task.FromResult(IdentityResult.Success);
            else
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
        }
    }
}
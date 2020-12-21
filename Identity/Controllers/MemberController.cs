using System;
using System.Threading.Tasks;
using Identity.Models;
using Identity.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public MemberController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var model = user.Adapt<UserViewModel>();

            return View(model);
        }

        public IActionResult PasswordChange()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PasswordChange(ChangePasswordViewModel model)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user is null)
                return BadRequest("Kullanıcı bulunamadı");

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordCorrect)
                return BadRequest("Şifreniz yanlış");

            if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest("Yeni şifreniz uyuşmuyor");

            var result = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);

                await _signInManager.SignOutAsync();
                await _signInManager.PasswordSignInAsync(user, model.NewPassword, true, false);

                return Redirect("Index");
            }
            else
                foreach (var error in result.Errors)
                    Console.WriteLine(error);

            return View();
        }
    }
}
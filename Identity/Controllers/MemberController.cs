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

            var phoneNumber = model.PhoneNumber;
            if (phoneNumber.Length == 10)
            {
                // 0 (xxx) xxx xx xx
                var phoneNumberMasked = $"0 ({phoneNumber[0]}{phoneNumber[1]}{phoneNumber[2]}) " +
                    $"{phoneNumber[3]}{phoneNumber[4]}{phoneNumber[5]} " +
                    $"{phoneNumber[6]}{phoneNumber[7]} {phoneNumber[8]}{phoneNumber[9]}";
                model.PhoneNumber = phoneNumberMasked;
            }

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

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordCorrect)
                return BadRequest("Şifreniz yanlış");

            if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest("Yeni şifreniz uyuşmuyor");

            var result = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);

                // Security Stamp'i güncellediğimiz için, Identity'nin arkaplanda yaptığı
                // kontrolde kullanıcının cookie'sinde eski security stamp olduğu görüp
                // çıkış yaptıracak. Bu olmasın diye çıkış yapıp tekrar giriş yapıyoruz.
                await _signInManager.SignOutAsync();
                await _signInManager.PasswordSignInAsync(user, model.NewPassword, true, false);

                return Redirect("Index");
            }
            else
                foreach (var error in result.Errors)
                    Console.WriteLine(error);

            return View();
        }

        public async Task<IActionResult> UserEdit()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var model = user.Adapt<UserViewModel>();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel model)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);

                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, true);

                return View(model);
            }

            foreach (var error in result.Errors)
                Console.WriteLine(error.Description);

            return BadRequest("Güncelleme sırasında hata oluştu");
        }
    }
}
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
    public class MemberController : BaseController
    {
        public MemberController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
            : base(userManager, signInManager)
        {
        }

        public IActionResult IndexAsync()
        {
            var model = CurrentUser.Adapt<UserViewModel>();

            var phoneNumber = model.PhoneNumber;
            if (phoneNumber != null && phoneNumber.Length == 10)
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
            var isPasswordCorrect = await _userManager.CheckPasswordAsync(CurrentUser, model.Password);
            if (!isPasswordCorrect)
                return BadRequest("Şifreniz yanlış");

            if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest("Yeni şifreniz uyuşmuyor");

            var result = await _userManager.ChangePasswordAsync(CurrentUser, model.Password, model.NewPassword);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(CurrentUser);

                // Security Stamp'i güncellediğimiz için, Identity'nin arkaplanda yaptığı
                // kontrolde kullanıcının cookie'sinde eski security stamp olduğu görüp
                // çıkış yaptıracak. Bu olmasın diye çıkış yapıp tekrar giriş yapıyoruz.
                await _signInManager.SignOutAsync();
                await _signInManager.PasswordSignInAsync(CurrentUser, model.NewPassword, true, false);

                return Redirect("Index");
            }
            else
                foreach (var error in result.Errors)
                    Console.WriteLine(error);

            return View();
        }

        public IActionResult UserEdit()
        {
            var model = CurrentUser.Adapt<UserViewModel>();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel model)
        {
            CurrentUser.UserName = model.UserName;
            CurrentUser.Email = model.Email;
            CurrentUser.PhoneNumber = model.PhoneNumber;
            CurrentUser.City = model.City;
            CurrentUser.Gender = model.Gender;
            CurrentUser.BirthDate = model.BirthDate;

            var result = await _userManager.UpdateAsync(CurrentUser);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(CurrentUser);

                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(CurrentUser, true);

                return View(model);
            }

            foreach (var error in result.Errors)
                Console.WriteLine(error.Description);

            return BadRequest("Güncelleme sırasında hata oluştu");
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize(Roles = "Editor, Admin")]
        public IActionResult Editor()
        {
            return View();
        }

        [Authorize(Roles = "Manager, Admin")]
        public IActionResult Manager()
        {
            return View();
        }
    }
}
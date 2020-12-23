using System;
using System.Threading.Tasks;
using Identity.Models;
using Identity.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;

namespace Identity.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IEmailService _emailService;

        public HomeController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailService emailService)
            : base(userManager, signInManager)
        {
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Member");

            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("{ReturnUrl}")]
        public IActionResult Login(string ReturnUrl)
        {
            // TempData, ViewBag ve ViewData'dan farklı olarak
            // tuttuğu bilgiye action'lar arası ulaşılabilir.
            TempData["ReturnUrl"] = ReturnUrl;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
                return BadRequest("Kullanıcı bulunamadı");

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return BadRequest("Giriş yapmak için email adresinizi doğrulayın.");

            if (await _userManager.IsLockedOutAsync(user))
                return BadRequest("Hesabınız kilitlenmiştir. 20 dakika sonra tekrar deneyin.");

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                false);

            if (result.Succeeded)
            {
                await _userManager.ResetAccessFailedCountAsync(user);

                if (TempData["ReturnUrl"] != null)
                    return Redirect(TempData["ReturnUrl"].ToString());

                return RedirectToAction("Index", "Member");
            }
            else
            {
                await _userManager.AccessFailedAsync(user);

                int failCount = await _userManager.GetAccessFailedCountAsync(user);
                Console.WriteLine($"{failCount} kez başarısız giriş denemesi yaptınız");

                if (failCount is 3)
                {
                    await _userManager.SetLockoutEndDateAsync(
                        user,
                        new DateTimeOffset(DateTime.UtcNow.AddMinutes(20)));

                    return BadRequest("3 başarısız giriş denemesi yaptığınız için hesabınız 20 dk kitlenmiştir");
                }
            }

            return BadRequest("Kullanıcı adı veya şifre yanlış");
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = new AppUser
            {
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var link = Url.Action(
                    "ConfirmEmail",
                    "Home",
                    new { userId = user.Id, token },
                    HttpContext.Request.Scheme);

                await _emailService.SendAsync(model.Email, "Email Doğrula", $"<a href=\"{link}\">Doğrula</a>");
                return RedirectToAction("Login");
            }

            return View();
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
                return BadRequest("Bu email adresine kayıtlı kullanıcı bulunamadı.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action(
                "ResetPasswordConfirm",
                "Home",
                new { userId = user.Id, token },
                HttpContext.Request.Scheme);

            await _emailService.SendAsync(model.Email, "Şifre Sıfırlama", $"<a href=\"{link}\">Sıfırla</a>");

            return View();
        }

        public IActionResult ResetPasswordConfirm(string userId, string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordConfirm(
            // Gelen model içerisinden sadece Password değişkenini alır.
            [Bind("Password")] ResetPasswordViewModel model)
        {
            var userId = TempData["userId"].ToString();
            var token = TempData["token"].ToString();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return BadRequest("Kullanıcı bulunamadı");

            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                Console.WriteLine("Şifreniz sıfırlanmıştır");

                return Redirect(nameof(Login));
            }
            else
            {
                foreach (var error in result.Errors)
                    Console.WriteLine(error);
            }

            return View();
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return RedirectToAction("Login");

            return BadRequest();
        }
    }
}
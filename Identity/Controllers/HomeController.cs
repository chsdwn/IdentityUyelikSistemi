using System;
using System.Threading.Tasks;
using Identity.Models;
using Identity.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public HomeController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
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
            // tuttuğu biliye action'lar arası ulaşılabilir.
            TempData["ReturnUrl"] = ReturnUrl;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
                return BadRequest();

            if (await _userManager.IsLockedOutAsync(user))
                Console.WriteLine("Hesabınız kilitlenmiştir. 20 dakika sonra tekrar deneyin.");

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

                    Console.WriteLine("3 başarısız giriş denemesi yaptığınız için hesabınız 20 dk kitlenmiştir");
                }
            }

            Console.WriteLine("Kullanıcı adı veya şifre yanlış");

            return View();
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
                return RedirectToAction("Login");

            return View();
        }
    }
}
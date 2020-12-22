using System.Linq;
using System.Threading.Tasks;
using Identity.Models;
using Identity.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    public class AdminController : BaseController
    {
        public AdminController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<AppRole> roleManager)
            : base(userManager, signInManager, roleManager)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View(_userManager.Users.ToList());
        }

        public IActionResult Roles()
        {
            return View(_roleManager.Roles.ToList());
        }

        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(RoleViewModel model)
        {
            var role = new AppRole { Name = model.Name };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
                return RedirectToAction(nameof(Roles));

            return BadRequest("Rol oluşturma sırasında hata oluştu.");
        }

        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
                return BadRequest("Rol Bulunamadı");

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return RedirectToAction(nameof(Roles));

            return BadRequest("Rol silinirken hata oluştu");
        }

        public async Task<IActionResult> UpdateRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
                return RedirectToAction(nameof(Roles));

            return View(role.Adapt<RoleViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(RoleViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role is null)
                return RedirectToAction(nameof(Roles));

            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
                return RedirectToAction(nameof(Roles));

            return BadRequest("Rol güncellenirken hata oluştu");
        }
    }
}
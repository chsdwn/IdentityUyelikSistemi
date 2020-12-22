using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.Models;
using Identity.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    [Authorize(Roles = "Admin")]
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

        public async Task<IActionResult> AssignRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return BadRequest("Kullanıcı bulunamadı");

            var roles = _roleManager.Roles;
            var userRoles = await _userManager.GetRolesAsync(user);
            var roleViewModels = new List<AssignRoleViewModel>();

            foreach (var role in roles)
            {
                var isChecked = false;
                if (userRoles.Contains(role.Name))
                    isChecked = true;

                roleViewModels.Add(new AssignRoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    IsChecked = isChecked
                });
            }

            return View(roleViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string id, List<AssignRoleViewModel> roles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return BadRequest("Kullanıcı bulunamadı");

            foreach (var role in roles)
            {
                if (role.IsChecked)
                {
                    if (!await _userManager.IsInRoleAsync(user, role.Name))
                        await _userManager.AddToRoleAsync(user, role.Name);
                }
                else
                {
                    if (await _userManager.IsInRoleAsync(user, role.Name))
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
            }

            return RedirectToAction(nameof(Users));

        }
    }
}
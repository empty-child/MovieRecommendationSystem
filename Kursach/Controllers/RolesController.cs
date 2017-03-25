using Kursach.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Kursach.Controllers
{
    public class RolesController : Controller
    {
        private ApplicationRoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
        }

        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        [Authorize(Roles = "admin")]
        public ActionResult Index()
        {
            return View(RoleManager.Roles);
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(CreateRoleModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await RoleManager.CreateAsync(new ApplicationRole
                {
                    Name = model.Name,
                    Description = model.Description
                });
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Что-то пошло не так");
                }
            }
            return View(model);
        }

        public async Task<ActionResult> Edit(string id)
        {
            ApplicationRole role = await RoleManager.FindByIdAsync(id);
            if (role != null)
            {
                return View(new EditRoleModel { Id = role.Id, Name = role.Name, Description = role.Description });
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<ActionResult> Edit(EditRoleModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationRole role = await RoleManager.FindByIdAsync(model.Id);
                if (role != null)
                {
                    role.Description = model.Description;
                    role.Name = model.Name;
                    IdentityResult result = await RoleManager.UpdateAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Что-то пошло не так");
                    }
                }
            }
            return View(model);
        }

        public async Task<ActionResult> Delete(string id)
        {
            ApplicationRole role = await RoleManager.FindByIdAsync(id);
            if (role != null)
            {
                IdentityResult result = await RoleManager.DeleteAsync(role);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult UserRole()
        {
            var users = UserManager.Users.ToList();
            var rolesList = RoleManager.Roles.ToList();
            SortedList<string, List<string>> userRoles = new SortedList<string, List<string>>();
            foreach (var user in users)
            {
                List<string> rolesArray = new List<string>(user.Roles.Count());
                foreach (var userRole in user.Roles)
                {
                    var role = RoleManager.FindById(userRole.RoleId);
                    rolesArray.Add(role.Name);
                }
                userRoles.Add(user.UserName, rolesArray);
            }

            var roles = RoleManager.Roles.Select(p => p.Name).ToList();

            SelectList rolesName = new SelectList(roles, "Name");

            ViewBag.rolesName = rolesName;
            ViewBag.userRoles = userRoles;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UserRole(string Name, string roleName)
        {
            ApplicationUser id = await UserManager.FindByNameAsync(Name);
            if (id != null)
            {
                IdentityResult result = await UserManager.AddToRoleAsync(id.Id, roleName);
            }
            return RedirectToAction("Index");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HelpDesk.DAL;
using HelpDesk.Models;
using HelpDesk.Entities;
using static HelpDesk.Infrastructure.Utilities;
using System.Linq.Expressions;
using PagedList;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;

namespace HelpDesk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private IUnitOfWork unitOfWork;
        private HelpDeskContext context;

        public UsersController()
        {
            unitOfWork = new UnitOfWork();
            context = new HelpDeskContext();
        }

        public ActionResult Index([Bind(Include = "Search,SortBy,DescSort,Page")] UsersIndexViewModel model)
        {
            Expression<Func<AppUser, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                filter = u => u.FirstName.ToLower().Contains(model.Search.ToLower()) ||
                              u.LastName.ToLower().Contains(model.Search.ToLower()) ||
                              u.Email.ToLower().Contains(model.Search.ToLower()) ||
                              u.Phone.ToLower().Contains(model.Search.ToLower()) ||
                              u.MobilePhone.ToLower().Contains(model.Search.ToLower()) ||
                              u.Company.ToLower().Contains(model.Search.ToLower()) ||
                              u.Department.ToLower().Contains(model.Search.ToLower());                
            }

            Func<IQueryable<AppUser>, IOrderedQueryable<AppUser>> orderBy = null;
            switch (model.SortBy)
            {
                case "FirstName":
                    orderBy = q => model.DescSort ? q.OrderByDescending(u => u.FirstName) : q.OrderBy(u => u.FirstName);
                    break;
                case "LastName":
                    orderBy = q => model.DescSort ? q.OrderByDescending(u => u.LastName) : q.OrderBy(u => u.LastName);
                    break;
                case "Email":
                    orderBy = q => model.DescSort ? q.OrderByDescending(u => u.Email) : q.OrderBy(u => u.Email);
                    break;
                case "Phone":
                    orderBy = q => model.DescSort ? q.OrderByDescending(u => u.Phone) : q.OrderBy(u => u.Phone);
                    break;
                case "MobilePhone":
                    orderBy = q => model.DescSort ? q.OrderByDescending(u => u.MobilePhone) : q.OrderBy(u => u.MobilePhone);
                    break;
                case "Company":
                    orderBy = q => model.DescSort ? q.OrderByDescending(u => u.Company) : q.OrderBy(u => u.Company);
                    break;
                case "Department":
                    orderBy = q => model.DescSort ? q.OrderByDescending(u => u.Department) : q.OrderBy(u => u.Department);
                    break;
                case "Role":
                    orderBy = q => model.DescSort ? q.OrderByDescending(u => u.Roles.FirstOrDefault().RoleId) : q.OrderBy(u => u.Roles.FirstOrDefault().RoleId);
                    break;
                case "Tickets":
                    orderBy = q => model.DescSort ? q.OrderByDescending(u => u.CreatedTickets.Count) : q.OrderBy(u => u.CreatedTickets.Count);
                    break;                    
            }

            IQueryable<AppUser> query = userManager.Users;
            if (filter != null)
                query = query.Where(filter);
            if (orderBy != null)
                query = orderBy(query);
            model.Users = query.ToPagedList(model.Page, 5);
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "FirstName,LastName,Email,Password,ConfirmPassword,Phone,MobilePhone,Company,Department,Role")] UsersCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AppUser user = new AppUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Phone = model.Phone,
                        MobilePhone = model.MobilePhone,
                        Company = model.Company,
                        Department = model.Department
                    };
                    AppRole role = HttpContext.GetOwinContext().GetUserManager<AppRoleManager>().FindByName(model.Role);
                    if (role == null)
                        HttpContext.GetOwinContext().GetUserManager<AppRoleManager>().Create(new AppRole { Name = model.Role });

                    IdentityResult result = await userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        userManager.AddToRole(user.Id, model.Role);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (string error in result.Errors)
                            ModelState.AddModelError("", error);

                    }
                }
            }
            catch
            {
                ModelState.AddModelError("", "Cannot add new user. Try again!");
            }
            return View(model);
        }

        public async Task<ActionResult> Edit(string id)
        {
            try
            {
                AppUser user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                UsersEditViewModel model = new UsersEditViewModel
                {
                    UserID = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    MobilePhone = user.MobilePhone,
                    Company = user.Company,
                    Department = user.Department,
                    Role = (await userManager.GetRolesAsync(user.Id))[0],
                    Tickets = user.CreatedTickets.OrderByDescending(t => t.CreatedOn)
                };
                return View(model);
            }
            catch
            {
                TempData["Fail"] = "Cannot get user's info. Try again!";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "UserID,FirstName,LastName,Email,Password,ConfirmPassword,Phone,MobilePhone,Company,Department,Role")] UsersEditViewModel model)
        {
            try
            {
                AppUser user = await userManager.FindByIdAsync(model.UserID);
                if (user == null)
                    return HttpNotFound();

                bool editingOwnAccount = false;
                if (user.Email == User.Identity.Name)
                    editingOwnAccount = true;

                if (ModelState.IsValid)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.UserName = model.Email;
                    user.Email = model.Email;
                    user.Phone = model.Phone;
                    user.MobilePhone = model.MobilePhone;
                    user.Company = model.Company;
                    user.Department = model.Department;

                    IdentityResult passwordValidationResult = null;
                    if (model.Password != null)
                    {
                        passwordValidationResult = await userManager.PasswordValidator.ValidateAsync(model.Password);
                        if (passwordValidationResult.Succeeded)
                            user.PasswordHash = userManager.PasswordHasher.HashPassword(model.Password);
                        else
                            foreach (string error in passwordValidationResult.Errors)
                                ModelState.AddModelError("", error);
                    }
                    if (model.Password == null || passwordValidationResult.Succeeded)
                    {
                        IdentityResult userUpdateResult = await userManager.UpdateAsync(user);
                        if (userUpdateResult.Succeeded)
                        {
                            AppRole role = roleManager.FindByName(model.Role);
                            if (role == null)
                                roleManager.Create(new AppRole { Name = model.Role });
                            userManager.RemoveFromRoles(user.Id, userManager.GetRoles(user.Id).ToArray<string>());
                            userManager.AddToRole(user.Id, model.Role);

                            if (editingOwnAccount)
                            {
                                IAuthenticationManager AuthManager = HttpContext.GetOwinContext().Authentication;
                                AuthManager.SignOut();
                                AuthManager.SignIn(await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie));
                            }
                            TempData["Success"] = "Successfully edited user!";
                            return RedirectToAction("Index");
                        }
                        else
                            foreach (string error in userUpdateResult.Errors)
                                ModelState.AddModelError("", error);
                    }
                }
                model.Tickets = user.CreatedTickets.OrderByDescending(t => t.CreatedOn);
                return View(model);
            }
            catch
            {
                TempData["Fail"] = "Cannot edit user. Try again!";
                return RedirectToAction("Index");
            }
        }

        [OverrideAuthorization]
        public async Task<ActionResult> EditSelf()
        {
            try
            {
                AppUser user = await userManager.FindByEmailAsync(User.Identity.Name);
                if (user == null)
                {
                    return HttpNotFound();
                }
                UsersEditSelfViewModel model = new UsersEditSelfViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    MobilePhone = user.MobilePhone,
                    Company = user.Company,
                    Department = user.Department
                };
                return View(model);
            }
            catch
            {
                TempData["Fail"] = "Cannot get user's info. Try again!";
                return RedirectToAction("Index");
            }
        }
        [OverrideAuthorization]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSelf([Bind(Include = "FirstName,LastName,Email,Password,ConfirmPassword,Phone,MobilePhone,Company,Department")] UsersEditSelfViewModel model)
        {
            try
            {
                AppUser user = await userManager.FindByEmailAsync(User.Identity.Name);
                if (user == null)
                    return HttpNotFound();

                if (ModelState.IsValid)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.UserName = model.Email;
                    user.Email = model.Email;
                    user.Phone = model.Phone;
                    user.MobilePhone = model.MobilePhone;
                    user.Company = model.Company;
                    user.Department = model.Department;

                    IdentityResult passwordValidationResult = null;
                    if (model.Password != null)
                    {
                        passwordValidationResult = await userManager.PasswordValidator.ValidateAsync(model.Password);
                        if (passwordValidationResult.Succeeded)
                            user.PasswordHash = userManager.PasswordHasher.HashPassword(model.Password);
                        else
                            foreach (string error in passwordValidationResult.Errors)
                                ModelState.AddModelError("", error);
                    }
                    if (model.Password == null || passwordValidationResult.Succeeded)
                    {
                        IdentityResult userUpdateResult = await userManager.UpdateAsync(user);
                        if (userUpdateResult.Succeeded)
                        {
                            IAuthenticationManager AuthManager = HttpContext.GetOwinContext().Authentication;
                            AuthManager.SignOut();
                            AuthManager.SignIn(await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie));
                            
                            TempData["Success"] = "Successfully edited user!";
                            return RedirectToAction("Index", "Home");
                        }
                        else
                            foreach (string error in userUpdateResult.Errors)
                                ModelState.AddModelError("", error);
                    }
                }
                return View(model);
            }
            catch
            {
                TempData["Fail"] = "Cannot edit user. Try again!";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                AppUser user = await userManager.Users.Include(u => u.CreatedTickets)
                                                      .Include(u => u.RequestedTickets)
                                                      .Include(u => u.AssignedTickets)
                                                      .SingleOrDefaultAsync(u => u.Id == id);
                if (user == null)
                    return HttpNotFound();

                bool deletingOwnAccount = user.Email == User.Identity.Name;

                IdentityResult userDeletionResult = await userManager.DeleteAsync(user);

                if (userDeletionResult.Succeeded)
                {
                    if (deletingOwnAccount)
                    {
                        IAuthenticationManager AuthManager = HttpContext.GetOwinContext().Authentication;
                        AuthManager.SignOut();
                    }
                    else
                        TempData["Success"] = "Successfully deleted user!";
                }
                else
                    TempData["Fail"] = "Cannot delete user. Try again!";
            }
            catch
            {
                TempData["Fail"] = "Cannot delete user. Try again!";
            }
            return RedirectToAction("Index");
        }

        private AppUserManager userManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }

        private AppRoleManager roleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppRoleManager>();
            }
        }
    }
}

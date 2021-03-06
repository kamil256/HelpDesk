﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using HelpDesk.DAL.Abstract;
using HelpDesk.UI.ViewModels.Users;
using HelpDesk.UI.Infrastructure;
using HelpDesk.UI.Infrastructure.Abstract;

namespace HelpDesk.UI.Controllers.MVC
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IIdentityHelper identityHelper;

        public UsersController(IUnitOfWork unitOfWork, IIdentityHelper identityHelper)
        {
            this.unitOfWork = unitOfWork;
            this.identityHelper = identityHelper;
        }

        public ViewResult Index()
        {
            return View("Index");
        }

        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "FirstName,LastName,Email,Password,ConfirmPassword,Phone,MobilePhone,Company,Department,Role")] CreateViewModel model)
        {
            if (model.Role != "Admin" && model.Role != "User")
                ModelState.AddModelError("Role", $"Role \"{model.Role}\" is incorrect.");

            try
            {
                if (ModelState.IsValid)
                {
                    User user = new User
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Phone = model.Phone,
                        MobilePhone = model.MobilePhone,
                        Company = model.Company,
                        Department = model.Department,
                        Settings = new Settings()
                    };
                    IdentityResult createUserResult = await identityHelper.UserManager.CreateAsync(user, model.Password);
                    if (createUserResult.Succeeded)
                    {
                        IdentityResult addUserToRoleResult;
                        if (model.Role == "Admin")
                            addUserToRoleResult = identityHelper.UserManager.AddToRole(user.Id, "Admin");
                        else
                            addUserToRoleResult = identityHelper.UserManager.AddToRole(user.Id, "User");
                        if (!addUserToRoleResult.Succeeded)
                            TempData["Fail"] = "Unable to add user to selected role. Try again, and if the problem persists contact your system administrator.";

                        TempData["Success"] = "Successfully created new user.";
                        return RedirectToAction("Edit", new { id = user.Id });
                    }
                    else
                        foreach (string error in createUserResult.Errors)
                            ModelState.AddModelError("", error);
                }
            }
            catch
            {
                TempData["Fail"] = "Unable to create new user. Try again, and if the problem persists contact your system administrator.";
            }

            return View(model);
        }

        [OverrideAuthorization]
        [Authorize]
        public async Task<ActionResult> Edit(string id)
        {
            User user;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                user = await identityHelper.UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Fail"] = "Unable to display user details. Try again, and if the problem persists contact your system administrator.";
                    return RedirectToAction("Index");
                }
            }
            else
                user = identityHelper.CurrentUser;

            EditViewModel model = new EditViewModel
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                MobilePhone = user.MobilePhone,
                Company = user.Company,
                Department = user.Department,
                Role = identityHelper.UserManager.IsInRole(user.Id, "Admin") ? "Admin" : "User",
                LastActivity = user.LastActivity != null ? ((DateTime)user.LastActivity).ToString("yyyy-MM-dd HH:mm") : "Never",
                Active = user.Active
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OverrideAuthorization]
        [Authorize]
        public async Task<ActionResult> Edit([Bind(Include = "UserId,FirstName,LastName,Email,Phone,MobilePhone,Company,Department,Role,Active")] EditViewModel model)
        {
            User user;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                user = await identityHelper.UserManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    TempData["Fail"] = "Unable to edit user. Try again, and if the problem persists contact your system administrator.";
                    return RedirectToAction("Index");
                }
                else if (HttpContext.User.Identity.Name == "demo@example.com" && identityHelper.IsUserAnAdministrator(user.Id))
                {
                    TempData["Fail"] = "Demo user can't edit own or other administrator's account.";
                    return View(model);
                }
                if (model.Role != "Admin" && model.Role != "User")
                    ModelState.AddModelError("Role", $"Role \"{model.Role}\" is incorrect.");
            }
            else
            {
                user = identityHelper.CurrentUser;
                ModelState.Remove("Role");
            }

            try
            {
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
                    user.Active = model.Active;

                    IdentityResult userUpdateResult = await identityHelper.UserManager.UpdateAsync(user);
                    if (userUpdateResult.Succeeded)
                    {                        
                        if (identityHelper.IsCurrentUserAnAdministrator())
                        {
                            IdentityResult addUserToRoleResult = identityHelper.UserManager.RemoveFromRoles(user.Id, identityHelper.UserManager.GetRoles(user.Id).ToArray<string>());
                            if (addUserToRoleResult.Succeeded)
                            {
                                if (model.Role == "Admin")
                                    addUserToRoleResult = identityHelper.UserManager.AddToRole(user.Id, "Admin");
                                else
                                    addUserToRoleResult = identityHelper.UserManager.AddToRole(user.Id, "User");
                            }
                            if (!addUserToRoleResult.Succeeded)
                                TempData["Fail"] = "Unable to edit user role. Try again, and if the problem persists contact your system administrator.";
                        }

                        if (user.Id == identityHelper.CurrentUser.Id)
                        {
                            HttpContext.GetOwinContext().Authentication.SignOut();
                            HttpContext.GetOwinContext().Authentication.SignIn(await identityHelper.UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie));
                        }

                        TempData["Success"] = "Successfully edited user.";
                        return RedirectToAction("Edit", new { id = user.Id });
                    }
                    else
                        foreach (string error in userUpdateResult.Errors)
                            ModelState.AddModelError("", error);
                }
            }
            catch
            {
                TempData["Fail"] = "Unable to edit user. Try again, and if the problem persists contact your system administrator.";
            }
            model.LastActivity = user.LastActivity != null ? ((DateTime)user.LastActivity).ToString("yyyy-MM-dd HH:mm") : "Never";
            return View(model);
        }

        [OverrideAuthorization]
        [Authorize]
        public async Task<ActionResult> ChangePassword(string id)
        {
            User user;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                user = await identityHelper.UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Fail"] = "Unable to change user's password. Try again, and if the problem persists contact your system administrator.";
                    return RedirectToAction("Index");
                }
            }
            else
                user = identityHelper.CurrentUser;

            ChangePasswordViewModel model = new ChangePasswordViewModel
            {
                UserId = user.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OverrideAuthorization]
        [Authorize]
        public async Task<ActionResult> ChangePassword([Bind(Include = "UserId,CurrentPassword,NewPassword,ConfirmPassword")] ChangePasswordViewModel model)
        {
            User user;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                user = await identityHelper.UserManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    TempData["Fail"] = "Unable to change user's password. Try again, and if the problem persists contact your system administrator.";
                    return RedirectToAction("Index");
                }
                else if (HttpContext.User.Identity.Name == "demo@example.com" && identityHelper.IsUserAnAdministrator(user.Id))
                {
                    TempData["Fail"] = "Demo user can't change own or other administrator's password.";
                    return View(model);
                }
                if (user.Id != identityHelper.CurrentUser.Id)
                {
                    ModelState.Remove("CurrentPassword");
                }
            }
            else
                user = identityHelper.CurrentUser;

            try
            {
                if (ModelState.IsValid)
                {
                    if (identityHelper.IsCurrentUserAnAdministrator() && user.Id != identityHelper.CurrentUser.Id)
                    {
                        IdentityResult validatePasswordResult = await identityHelper.UserManager.PasswordValidator.ValidateAsync(model.NewPassword);
                        if (validatePasswordResult.Succeeded)
                        {
                            user.PasswordHash = identityHelper.UserManager.PasswordHasher.HashPassword(model.NewPassword);
                            IdentityResult updateSecurityStampResult = await identityHelper.UserManager.UpdateSecurityStampAsync(user.Id);
                            if (updateSecurityStampResult.Succeeded)
                            {
                                IdentityResult changePasswordResult = await identityHelper.UserManager.UpdateAsync(user);
                                if (changePasswordResult.Succeeded)
                                {
                                    TempData["Success"] = "Successfully changed password!";
                                    return RedirectToAction("ChangePassword", new { id = user.Id });
                                }
                                else
                                    foreach (string error in validatePasswordResult.Errors)
                                        ModelState.AddModelError("", error);
                            }
                            else
                                foreach (string error in validatePasswordResult.Errors)
                                    ModelState.AddModelError("", error);
                        }
                        else
                            foreach (string error in validatePasswordResult.Errors)
                                ModelState.AddModelError("", error);
                    }
                    else
                    {
                        IdentityResult changePasswordResult = await identityHelper.UserManager.ChangePasswordAsync(user.Id, model.CurrentPassword, model.NewPassword);
                        if (changePasswordResult.Succeeded)
                        {
                            TempData["Success"] = "Successfully changed password!";
                            return RedirectToAction("ChangePassword", new { id = user.Id });
                        }
                        else
                            foreach (string error in changePasswordResult.Errors)
                                ModelState.AddModelError("", error);
                    }
                }
            }
            catch
            {
                TempData["Fail"] = "Unable to change user's password. Try again, and if the problem persists contact your system administrator.";
            }
            return View(model);
        }

        [OverrideAuthorization]
        [Authorize]
        public async Task<ActionResult> Tickets(string id)
        {
            TicketsViewModel model = new TicketsViewModel();

            User user;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                user = await identityHelper.UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Fail"] = "Unable to list user's tickets. Try again, and if the problem persists contact your system administrator.";
                    return RedirectToAction("Index");
                }
            }
            else
                user = identityHelper.CurrentUser;
            model.UserId = user.Id;

            string adminRoleId = identityHelper.RoleManager.FindByName("Admin").Id;
            model.Administrators = identityHelper.UserManager.Users.Where(u => u.Roles.FirstOrDefault(r => r.RoleId == adminRoleId) != null).Select(u => new AdministratorDTO
            {
                UserId = u.Id,
                Name = u.FirstName + " " + u.LastName
            }).OrderBy(u => u.Name).ToList();
            model.Administrators.Insert(0, new AdministratorDTO
            {
                UserId = "0",
                Name = "-"
            });

            return View("Tickets", model);
        }

        [OverrideAuthorization]
        [Authorize]
        public async Task<ActionResult> History(string id)
        {
            User user;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                user = await identityHelper.UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Fail"] = "Unable to list user's tickets. Try again, and if the problem persists contact your system administrator.";
                    return RedirectToAction("Index");
                }
            }
            else
                user = identityHelper.CurrentUser;

            HistoryViewModel model = new HistoryViewModel
            {
                UserId = id,
                Logs = new List<HistoryViewModel.Log>()
            };
            foreach (var log in unitOfWork.TicketsHistoryRepository.Get(filters: new Expression<Func<TicketsHistory, bool>>[] { l => l.AuthorId == user.Id }, orderBy: q => q.OrderByDescending(l => l.Date)))
            {
                model.Logs.Add(new HistoryViewModel.Log
                {
                    Date = log.Date,
                    TicketId = log.TicketId,
                    Column = log.Column,
                    NewValue = log.NewValue
                });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string userId)
        {
            
            User user = await identityHelper.UserManager.Users.Include(u => u.CreatedTickets)
                                                              .Include(u => u.RequestedTickets)
                                                              .Include(u => u.AssignedTickets)
                                                              .Include(u => u.Settings)
                                                              .SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                TempData["Fail"] = "Unable to delete user. Try again, and if the problem persists contact your system administrator.";
                return RedirectToAction("Index");
            }
            else if (HttpContext.User.Identity.Name == "demo@example.com" && identityHelper.IsUserAnAdministrator(user.Id))
            {
                TempData["Fail"] = "Demo user can't delete own or other administrator's account.";
                return RedirectToAction("Index");
            }

            try
            {
                IdentityResult userDeleteResult = await identityHelper.UserManager.DeleteAsync(user);

                if (userDeleteResult.Succeeded)
                {
                    TempData["Success"] = "Successfully deleted ticket.";

                    if (user.UserName == User.Identity.Name)
                    {
                        IAuthenticationManager AuthenticationManager = HttpContext.GetOwinContext().Authentication;
                        AuthenticationManager.SignOut();
                    }
                }                
            }
            catch
            {
                TempData["Fail"] = "Unable to delete user. Try again, and if the problem persists contact your system administrator.";
            }
            return RedirectToAction("Index");
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                filterContext.Result = new RedirectResult("~/Content/Error.html");
                filterContext.ExceptionHandled = true;
            }
        }
    }
}

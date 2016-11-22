﻿using System;
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
using System.Linq.Expressions;
using PagedList;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using HelpDesk.Models.Tickets;

namespace HelpDesk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }

        private AppRoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppRoleManager>();
            }
        }

        private AppUser CurrentUser
        {
            get
            {
                return UserManager.FindByNameAsync(User.Identity.Name).Result;
            }
        }

        private IUnitOfWork unitOfWork;

        public UsersController()
        {
            unitOfWork = new UnitOfWork();
        }

        public ViewResult Index()
        {
            return View("ApiIndex");
        }

        public ViewResult Create()
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
                        Department = model.Department,
                        Settings = new Settings()
                    };
                    IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        IdentityResult result2 = UserManager.AddToRole(user.Id, model.Role);
                        if (result2.Succeeded)
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            foreach (string error in result2.Errors)
                                ModelState.AddModelError("", error);

                        }
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

        //public ViewResult Details(string id)
        //{
        //    AppUser user = unitOfWork.UserRepository.GetById(id);
        //    UsersDetailsViewModel model = new UsersDetailsViewModel
        //    {
        //        UserID = user.Id,
        //        FirstName = user.FirstName,
        //        LastName = user.LastName,
        //        Email = user.Email,
        //        Phone = user.Phone,
        //        MobilePhone = user.MobilePhone,
        //        Company = user.Company,
        //        Department = user.Department,
        //        Role = unitOfWork.RoleRepository.GetById(user.Roles.First().RoleId).Name
        //    };
        //    return View(model);
        //}

        [OverrideAuthorization]
        public async Task<ActionResult> Edit(string id)
        {
            try
            {
                AppUser user;
                if (await UserManager.IsInRoleAsync(CurrentUser.Id, "Admin"))
                    user = await UserManager.FindByIdAsync(id);
                else
                    user = CurrentUser;
                if (user == null)
                    throw new Exception($"User id {id} doesn't exist");
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
                    Role = (await UserManager.IsInRoleAsync(user.Id, "Admin")) ? "Admin" : "User"
                };
                return View(model);
            }
            catch
            {
                TempData["Fail"] = "Poblem with editing user. Try again!";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OverrideAuthorization]
        public async Task<ActionResult> Edit([Bind(Include = "UserID,FirstName,LastName,Email,Phone,MobilePhone,Company,Department,Role")] UsersEditViewModel model)
        {
            AppUser user;
            try
            {
                if (await UserManager.IsInRoleAsync(CurrentUser.Id, "Admin"))
                    user = await UserManager.FindByIdAsync(model.UserID);
                else
                {
                    user = CurrentUser;
                    ModelState.Remove("Role");
                }

                if (user == null)
                    throw new Exception($"User id {model.UserID} doesn't exist");

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

                    IdentityResult userUpdateResult = await UserManager.UpdateAsync(user);
                    if (userUpdateResult.Succeeded)
                    {
                        if (await UserManager.IsInRoleAsync(CurrentUser.Id, "Admin"))
                        {
                            UserManager.RemoveFromRoles(user.Id, UserManager.GetRoles(user.Id).ToArray<string>());
                            if (model.Role == "Admin")
                                UserManager.AddToRole(user.Id, "Admin");
                            else
                                UserManager.AddToRole(user.Id, "User");
                        }
                        TempData["Success"] = "Successfully edited user!";
                        return RedirectToAction("Index");
                    }
                    else
                        foreach (string error in userUpdateResult.Errors)
                            ModelState.AddModelError("", error);
                }

            }
            catch
            {
                TempData["Fail"] = "Poblem with editing user. Try again!";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [OverrideAuthorization]
        public async Task<ActionResult> ChangePassword(string id)
        {
            try
            {
                AppUser user;
                if (await UserManager.IsInRoleAsync(CurrentUser.Id, "Admin"))
                    user = await UserManager.FindByIdAsync(id);
                else
                    user = CurrentUser;
                if (user == null)
                    throw new Exception($"User id {id} doesn't exist");
                UsersChangePasswordViewModel model = new UsersChangePasswordViewModel
                {
                    UserID = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
                return View(model);
            }
            catch
            {
                TempData["Fail"] = "Poblem with editing user. Try again!";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OverrideAuthorization]
        public async Task<ActionResult> ChangePassword([Bind(Include = "UserID,CurrentPassword,Password,ConfirmPassword")] UsersChangePasswordViewModel model)
        {
            AppUser user;
            try
            {
                if (await UserManager.IsInRoleAsync(CurrentUser.Id, "Admin"))
                    user = await UserManager.FindByIdAsync(model.UserID);
                else
                {
                    user = CurrentUser;
                    ModelState.Remove("Role");
                }
                if (user == null)
                    throw new Exception($"User id {model.UserID} doesn't exist");

                bool editingLoggedInUser = user.Email == User.Identity.Name;

                bool correctPassword = UserManager.CheckPassword(user, model.CurrentPassword);
                if (!correctPassword)
                    ModelState.AddModelError("CurrentPassword", "Incorrect current password.");


                if (ModelState.IsValid)
                {
                    IdentityResult changePasswordResult = await UserManager.ChangePasswordAsync(user.Id, model.CurrentPassword, model.Password);
                    if (changePasswordResult.Succeeded)
                    {
                        if (editingLoggedInUser)
                        {
                            IAuthenticationManager AuthManager = HttpContext.GetOwinContext().Authentication;
                            AuthManager.SignOut();
                            AuthManager.SignIn(await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie));
                        }
                        TempData["Success"] = "Successfully changed password!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                        foreach (string error in changePasswordResult.Errors)
                            ModelState.AddModelError("", error);

                }
            }
            catch
            {
                TempData["Fail"] = "Poblem with editing user. Try again!";
                return RedirectToAction("Index", "Home");
            }
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            return View(model);
        }

        public async Task<ActionResult> Tickets(string id)
        {
            try
            {
                AppUser user;
                if (await UserManager.IsInRoleAsync(CurrentUser.Id, "Admin"))
                    user = await UserManager.FindByIdAsync(id);
                else
                    user = CurrentUser;
                if (user == null)
                    throw new Exception($"User id {id} doesn't exist");

                UsersEditViewModel model = new UsersEditViewModel();
                model.UserID = user.Id;
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                model.Tickets = unitOfWork.TicketRepository.Get(filters: new List<Expression<Func<Ticket, bool>>> { t => t.CreatedByID == id }, orderBy: t => t.OrderByDescending(x => x.CreatedOn)).Select(t => new TicketDTO
                {
                    TicketId = t.TicketID,
                    CreatedOn = ((t.CreatedOn - new DateTime(1970, 1, 1)).Ticks / 10000).ToString(),
                    CreatedBy = t.CreatedBy != null ? t.CreatedBy.FirstName + " " + t.CreatedBy.LastName : null,
                    RequestedBy = t.RequestedBy != null ? t.RequestedBy.FirstName + " " + t.RequestedBy.LastName : null,
                    AssignedTo = t.AssignedTo != null ? t.AssignedTo.FirstName + " " + t.AssignedTo.LastName : null,
                    CreatedById = t.CreatedByID,
                    RequestedById = t.RequestedByID,
                    AssignedToId = t.AssignedToID,
                    Title = t.Title,
                    Category = t.Category?.Name,
                    Status = t.Status
                });
                return View("Tickets", model);
            }
            catch
            {
                TempData["Fail"] = "Poblem with editing user. Try again!";
                return RedirectToAction("Index", "Home");
            }
        }

        [OverrideAuthorization]
        public async Task<ActionResult> History(string id)
        {
            
            try
            {
                AppUser user = await UserManager.FindByIdAsync(id);
                if (user == null)
                    throw new Exception($"User id {id} doesn't exist");

                if (!await UserManager.IsInRoleAsync(CurrentUser.Id, "Admin") && user.Id != CurrentUser.Id)
                    return new HttpUnauthorizedResult();

                UsersHistoryViewModel model = new UsersHistoryViewModel
                {
                    UserID = id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Logs = new List<Log>()
                };
                foreach (var log in unitOfWork.AspNetUsersHistoryRepository.Get(filters: new Expression<Func<AspNetUsersHistory, bool>>[] { l => l.UserId == user.Id }, orderBy: x => x.OrderByDescending(l => l.ChangeDate)))
                {
                    AppUser changeAuthor = UserManager.FindById(log.ChangeAuthorId);
                    string logContent = String.Format("User [{0}] with ID [{1}] ", changeAuthor != null ? changeAuthor.FirstName + " " + changeAuthor.LastName : "deleted user", log.ChangeAuthorId);
                    switch (log.ActionType)
                    {
                        case "UPDATE":
                            logContent += $"changed [{log.ColumnName}] from [{log.OldValue}] to [{log.NewValue}]";
                            break;
                        case "INSERT":
                            logContent += "created user";
                            break;
                        case "DELETE":
                            logContent += "deleted user";
                            break;
                    }
                    model.Logs.Add(new Log
                    {
                        Date = log.ChangeDate,
                        Content = logContent}
                    );
                }
                return View(model);
            }
            catch
            {
                TempData["Fail"] = "Poblem with reading user history. Try again!";
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                AppUser user = await UserManager.Users.Include(u => u.CreatedTickets)
                                                      .Include(u => u.RequestedTickets)
                                                      .Include(u => u.AssignedTickets)
                                                      .Include(u => u.Settings)
                                                      .SingleOrDefaultAsync(u => u.Id == id);
                if (user == null)
                    return HttpNotFound();

                IdentityResult userDeletionResult = await UserManager.DeleteAsync(user);

                if (userDeletionResult.Succeeded)
                {
                    if (user.UserName == User.Identity.Name)
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
    }
}

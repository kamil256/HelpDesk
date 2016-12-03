using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using PagedList;
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

namespace HelpDesk.UI.Controllers.MVC
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IdentityHelper identityHelper;

        public UsersController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            this.identityHelper = new IdentityHelper();
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

                    if (addUserToRoleResult.Succeeded)
                        return RedirectToAction("Index");
                    else
                        foreach (string error in addUserToRoleResult.Errors)
                            ModelState.AddModelError("", error);
                }
                else
                    foreach (string error in createUserResult.Errors)
                        ModelState.AddModelError("", error);
            }
            return View(model);
        }

        [Authorize]
        public async Task<ActionResult> Edit(string id)
        {
            User user;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                user = await identityHelper.UserManager.FindByIdAsync(id);
                if (user == null)
                    return RedirectToAction("Index");
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
                Role = identityHelper.UserManager.IsInRole(user.Id, "Admin") ? "Admin" : "User"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Edit([Bind(Include = "UserId,FirstName,LastName,Email,Phone,MobilePhone,Company,Department,Role")] EditViewModel model)
        {
            User user;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                user = await identityHelper.UserManager.FindByIdAsync(model.UserId);
                if (user == null)
                    return RedirectToAction("Index");
            }
            else
            {
                user = identityHelper.CurrentUser;
                ModelState.Remove("Role");
            }
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

                IdentityResult userUpdateResult = await identityHelper.UserManager.UpdateAsync(user);
                if (userUpdateResult.Succeeded)
                {
                    if (identityHelper.IsCurrentUserAnAdministrator())
                    {
                        identityHelper.UserManager.RemoveFromRoles(user.Id, identityHelper.UserManager.GetRoles(user.Id).ToArray<string>());
                        if (model.Role == "Admin")
                            identityHelper.UserManager.AddToRole(user.Id, "Admin");
                        else
                            identityHelper.UserManager.AddToRole(user.Id, "User");
                    }
                    return RedirectToAction("Index");
                }
                else
                    foreach (string error in userUpdateResult.Errors)
                        ModelState.AddModelError("", error);
            }
            return View(model);
        }

        [Authorize]
        public async Task<ActionResult> ChangePassword(string id)
        {
            User user;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                user = await identityHelper.UserManager.FindByIdAsync(id);
                if (user == null)
                    return RedirectToAction("Index");
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
        [Authorize]
        public async Task<ActionResult> ChangePassword([Bind(Include = "UserId,CurrentPassword,NewPassword,ConfirmPassword")] ChangePasswordViewModel model)
        {
            User user;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                user = await identityHelper.UserManager.FindByIdAsync(model.UserId);
                if (user == null)
                    return RedirectToAction("Index");
            }
            else
            {
                user = identityHelper.CurrentUser;
            }

            //bool editingLoggedInUser = user.Email == User.Identity.Name;

            //bool passwordIsCorrect = UserManager.CheckPassword(user, model.CurrentPassword);
            //if (!passwordIsCorrect)
            //    ModelState.AddModelError("CurrentPassword", "Incorrect current password.");


            if (ModelState.IsValid)
            {
                IdentityResult changePasswordResult = await identityHelper.UserManager.ChangePasswordAsync(user.Id, model.CurrentPassword, model.NewPassword);
                if (changePasswordResult.Succeeded)
                {
                    //if (editingLoggedInUser)
                    //{
                    //    IAuthenticationManager AuthManager = HttpContext.GetOwinContext().Authentication;
                    //    AuthManager.SignOut();
                    //    AuthManager.SignIn(await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie));
                    //}
                    //TempData["Success"] = "Successfully changed password!";
                    return RedirectToAction("Index", "Home");
                }
                //else
                //    foreach (string error in changePasswordResult.Errors)
                //        ModelState.AddModelError("", error);

            }
            return View(model);
        }

        public async Task<ActionResult> Tickets(string id)
        {
            User user;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                user = await identityHelper.UserManager.FindByIdAsync(id);
                if (user == null)
                    return RedirectToAction("Index");
            }
            else
            {
                user = identityHelper.CurrentUser;
            }
            return View("Tickets", (object)user.Id);
        }

        [Authorize]
        public async Task<ActionResult> History(string id)
        {
            User user;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                user = await identityHelper.UserManager.FindByIdAsync(id);
                if (user == null)
                    return RedirectToAction("Index");
            }
            else
            {
                user = identityHelper.CurrentUser;
            }

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
            {
                User user = await identityHelper.UserManager.Users.Include(u => u.CreatedTickets)
                                                                  .Include(u => u.RequestedTickets)
                                                                  .Include(u => u.AssignedTickets)
                                                                  .Include(u => u.Settings)
                                                                  .SingleOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    RedirectToAction("Index");

                IdentityResult userDeleteResult = await identityHelper.UserManager.DeleteAsync(user);

                if (userDeleteResult.Succeeded)
                {
                    if (user.UserName == User.Identity.Name)
                    {
                        IAuthenticationManager AuthenticationManager = HttpContext.GetOwinContext().Authentication;
                        AuthenticationManager.SignOut();
                    }
                }
                return RedirectToAction("Index");
            }
        }    
    }
}

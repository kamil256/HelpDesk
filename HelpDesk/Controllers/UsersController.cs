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
using HelpDesk.Models.Tickets;

namespace HelpDesk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private IUnitOfWork unitOfWork;
        private HelpDeskContext context;

        public UsersController()
        {
            unitOfWork = new UnitOfWork();// HttpContext.GetOwinContext().GetUserManager<AppUserManager>());
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
            return View("ApiIndex"/*model*/);
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

        public ViewResult Details(string id)
        {
            AppUser user = unitOfWork.UserRepository.GetById(id);
            UsersDetailsViewModel model = new UsersDetailsViewModel
            {
                UserID = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                MobilePhone = user.MobilePhone,
                Company = user.Company,
                Department = user.Department,
                Role = unitOfWork.RoleRepository.GetById(user.Roles.First().RoleId).Name
            };
            return View(model);
        }

        public async Task<ActionResult> Tickets(string id)
        {
            try
            {
                AppUser user;
                if (await isCurrentUserAdmin())
                    user = await userManager.FindByIdAsync(id);
                else
                    user = await getCurrentUser();
                if (user == null)
                    throw new Exception($"User id {id} doesn't exist");
                UsersEditViewModel model = new UsersEditViewModel();
                model.UserID = user.Id;
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
        public async Task<ActionResult> Edit(string id)
        {
            try
            {
                AppUser user;
                if (await isCurrentUserAdmin())
                    user = await userManager.FindByIdAsync(id);
                else
                    user = await getCurrentUser();
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
                    Role = (await userManager.GetRolesAsync(user.Id))[0]
                    //Tickets = user.CreatedTickets.OrderByDescending(t => t.CreatedOn)
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
                if (await isCurrentUserAdmin())
                    user = await userManager.FindByIdAsync(model.UserID);
                else
                {
                    user = await getCurrentUser();
                    ModelState.Remove("Role");
                }

                if (user == null)
                    throw new Exception($"User id {model.UserID} doesn't exist");

                bool editingLoggedInUser = user.Email == User.Identity.Name;

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

                    IdentityResult userUpdateResult = await userManager.UpdateAsync(user);
                    if (userUpdateResult.Succeeded)
                    {
                        if (await isCurrentUserAdmin())
                        {
                            AppRole role = roleManager.FindByName(model.Role);
                            if (role == null)
                                roleManager.Create(new AppRole { Name = model.Role });
                            userManager.RemoveFromRoles(user.Id, userManager.GetRoles(user.Id).ToArray<string>());
                            userManager.AddToRole(user.Id, model.Role);
                        }

                        //if (editingLoggedInUser)
                        //{
                        //    IAuthenticationManager AuthManager = HttpContext.GetOwinContext().Authentication;
                        //    AuthManager.SignOut();
                        //    AuthManager.SignIn(await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie));
                        //}
                        TempData["Success"] = "Successfully edited user!";
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
            //model.Tickets = user.CreatedTickets.OrderByDescending(t => t.CreatedOn);
            return View(model);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[OverrideAuthorization]
        //public async Task<ActionResult> Edit([Bind(Include = "UserID,FirstName,LastName,Email,Password,ConfirmPassword,Phone,MobilePhone,Company,Department,Role")] UsersEditViewModel model)
        //{
        //    AppUser user;
        //    try
        //    {
        //        if (await isCurrentUserAdmin())
        //            user = await userManager.FindByIdAsync(model.UserID);
        //        else
        //        {
        //            user = await getCurrentUser();
        //            ModelState.Remove("Role");
        //        }

        //        if (user == null)
        //            throw new Exception($"User id {model.UserID} doesn't exist");

        //        bool editingLoggedInUser = user.Email == User.Identity.Name;

        //        if (ModelState.IsValid)
        //        {
        //            user.FirstName = model.FirstName;
        //            user.LastName = model.LastName;
        //            user.UserName = model.Email;
        //            user.Email = model.Email;
        //            user.Phone = model.Phone;
        //            user.MobilePhone = model.MobilePhone;
        //            user.Company = model.Company;
        //            user.Department = model.Department;

        //            IdentityResult passwordValidationResult = null;
        //            if (model.Password != null)
        //            {
        //                passwordValidationResult = await userManager.PasswordValidator.ValidateAsync(model.Password);
        //                if (passwordValidationResult.Succeeded)
        //                    user.PasswordHash = userManager.PasswordHasher.HashPassword(model.Password);
        //                else
        //                    foreach (string error in passwordValidationResult.Errors)
        //                        ModelState.AddModelError("", error);
        //            }
        //            if (model.Password == null || passwordValidationResult.Succeeded)
        //            {
        //                IdentityResult userUpdateResult = await userManager.UpdateAsync(user);
        //                if (userUpdateResult.Succeeded)
        //                {
        //                    if (await isCurrentUserAdmin())
        //                    {
        //                        AppRole role = roleManager.FindByName(model.Role);
        //                        if (role == null)
        //                            roleManager.Create(new AppRole { Name = model.Role });
        //                        userManager.RemoveFromRoles(user.Id, userManager.GetRoles(user.Id).ToArray<string>());
        //                        userManager.AddToRole(user.Id, model.Role);
        //                    }

        //                    if (editingLoggedInUser)
        //                    {
        //                        IAuthenticationManager AuthManager = HttpContext.GetOwinContext().Authentication;
        //                        AuthManager.SignOut();
        //                        AuthManager.SignIn(await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie));
        //                    }
        //                    TempData["Success"] = "Successfully edited user!";
        //                    return RedirectToAction("Index", "Home");
        //                }
        //                else
        //                    foreach (string error in userUpdateResult.Errors)
        //                        ModelState.AddModelError("", error);
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        TempData["Fail"] = "Poblem with editing user. Try again!";
        //        return RedirectToAction("Index", "Home");
        //    }
        //    //model.Tickets = user.CreatedTickets.OrderByDescending(t => t.CreatedOn);
        //    return View(model);
        //}

        [OverrideAuthorization]
        public async Task<ActionResult> ChangePassword(string id)
        {
            try
            {
                AppUser user;
                if (await isCurrentUserAdmin())
                    user = await userManager.FindByIdAsync(id);
                else
                    user = await getCurrentUser();
                if (user == null)
                    throw new Exception($"User id {id} doesn't exist");
                UsersChangePasswordViewModel model = new UsersChangePasswordViewModel
                {
                    UserID = user.Id
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
                if (await isCurrentUserAdmin())
                    user = await userManager.FindByIdAsync(model.UserID);
                else
                {
                    user = await getCurrentUser();
                    ModelState.Remove("Role");
                }

                if (user == null)
                    throw new Exception($"User id {model.UserID} doesn't exist");

                bool editingLoggedInUser = user.Email == User.Identity.Name;

                bool correctPassword = userManager.CheckPassword(user, model.CurrentPassword);
                if (!correctPassword)
                    ModelState.AddModelError("CurrentPassword", "Incorrect current password.");
                

                if (ModelState.IsValid)
                {
                    IdentityResult passwordValidationResult = await userManager.PasswordValidator.ValidateAsync(model.Password);
                    if (passwordValidationResult.Succeeded)
                    {
                        user.PasswordHash = userManager.PasswordHasher.HashPassword(model.Password);
                        userManager.Update(user);
                        if (editingLoggedInUser)
                        {
                            IAuthenticationManager AuthManager = HttpContext.GetOwinContext().Authentication;
                            AuthManager.SignOut();
                            AuthManager.SignIn(await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie));
                        }
                        TempData["Success"] = "Successfully edited user!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                        foreach (string error in passwordValidationResult.Errors)
                            ModelState.AddModelError("", error);
                    
                }
            }
            catch
            {
                TempData["Fail"] = "Poblem with editing user. Try again!";
                return RedirectToAction("Index", "Home");
            }
            //model.Tickets = user.CreatedTickets.OrderByDescending(t => t.CreatedOn);
            return View(model);
        }

        public async Task<ActionResult> History(string id)
        {
            
            try
            {
                AppUser user = await userManager.FindByIdAsync(id);
                if (user == null)
                    throw new Exception($"User id {id} doesn't exist");

                AppUser currentUser = await getCurrentUser();
                if (!await isCurrentUserAdmin() && user.Id != currentUser.Id)
                    return new HttpUnauthorizedResult();

                UsersHistoryViewModel model = new UsersHistoryViewModel
                {
                    UserID = id,
                    Logs = new List<Log>()
                };
                foreach (var log in context.AspNetUsersHistory.Where(l => l.UserId == user.Id).OrderByDescending(l => l.ChangeDate))
                {
                    AppUser changeAuthor = unitOfWork.UserRepository.GetById(log.ChangeAuthorId);
                    string logContent = String.Format("User [{0}] with ID [{1}] ", changeAuthor != null ? changeAuthor.FirstName + " " + changeAuthor.LastName : "deleted user", log.ChangeAuthorId);
                    switch (log.ActionType)
                    {
                        case "UPDATE":
                            logContent += $"updated value [{log.ColumnName}] from [{log.OldValue}] to [{log.NewValue}]";
                            break;
                        case "CREATE":
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

        private async Task<AppUser> getCurrentUser()
        {
            return await userManager.FindByEmailAsync(User.Identity.Name);
        }

        private async Task<bool> isCurrentUserAdmin()
        {
            return await userManager.IsInRoleAsync((await getCurrentUser()).Id, "Admin");
        }
    }
}

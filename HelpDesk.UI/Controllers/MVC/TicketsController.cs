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
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Text;
using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Entities;
using HelpDesk.UI.ViewModels;

namespace HelpDesk.UI.Controllers.MVC
{
    [Authorize(Roles = "Admin")]
    public class TicketsController : Controller
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

        public TicketsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [OverrideAuthorization]
        public ViewResult Index()
        {
            return View("Index");
        }

        [OverrideAuthorization]
        public ViewResult Create()
        {
            TicketsCreateViewModel model = new TicketsCreateViewModel
            {
                RequestedBy = CurrentUser,
                RequestedByID = CurrentUser.Id,
                Categories = unitOfWork.CategoryRepository.Get(orderBy: x => x.OrderBy(c => c.Order))
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OverrideAuthorization]
        public async Task<ActionResult> Create([Bind(Include = "RequestedByID,CategoryID,Title,Content")] TicketsCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Ticket ticket = new Ticket
                    {
                        CreatedByID = CurrentUser.Id,
                        RequestedByID = model.RequestedByID,
                        CreatedOn = DateTime.Now,
                        Status = "New",
                        CategoryID = model.CategoryID,
                        Title = model.Title,
                        Content = model.Content
                    };
                    unitOfWork.TicketRepository.Insert(ticket);
                    unitOfWork.Save();
                    TempData["Success"] = "Successfully added new ticket!";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                ModelState.AddModelError("", "Cannot create ticket. Try again!");
            }
            model.RequestedBy = await UserManager.FindByIdAsync(model.RequestedByID);
            model.Categories = unitOfWork.CategoryRepository.Get(orderBy: x => x.OrderBy(c => c.Order));
            return View(model);
        }

        [OverrideAuthorization]
        public async Task<ActionResult> Edit(int? id)
        {
            Ticket ticket = unitOfWork.TicketRepository.GetById(id ?? 0);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            if (!await UserManager.IsInRoleAsync(CurrentUser.Id, "Admin") && ticket.CreatedByID != CurrentUser.Id)
            {
                TempData["Fail"] = "You can't modify ticket which you didn't create!";
                return RedirectToAction("Index", "Home");
            }
            TicketsEditViewModel model = new TicketsEditViewModel
            {
                TicketID = ticket.TicketID,
                RequestedByID = ticket.RequestedByID,
                AssignedToID = ticket.AssignedToID,
                Status = ticket.Status,
                CategoryID = ticket.CategoryID,
                Title = ticket.Title,
                Content = ticket.Content,
                Solution = ticket.Solution,
                CreatedBy = ticket.CreatedBy,
                CreatedOn = ticket.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                RequestedBy = ticket.RequestedBy,                
                AssignedTo = ticket.AssignedTo
            };
            string adminRoleId = RoleManager.FindByName("Admin").Id;
            model.Admins = UserManager.Users.Where(u => u.Roles.FirstOrDefault(x => x.RoleId == adminRoleId) != null).OrderBy(u => u.FirstName);
            model.Categories = unitOfWork.CategoryRepository.Get(orderBy: x => x.OrderBy(c => c.Order));
            ViewBag.IsCurrentUserAdmin = await UserManager.IsInRoleAsync(CurrentUser.Id, "Admin");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OverrideAuthorization]
        public async Task<ActionResult> Edit([Bind(Include = "TicketID,RequestedByID,AssignedToID,Status,CategoryID,Title,Content,Solution")] TicketsEditViewModel model)
        {
            Ticket ticket = unitOfWork.TicketRepository.GetById(model.TicketID);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            model.RequestedBy = await UserManager.FindByIdAsync(model.RequestedByID);
            if (await UserManager.IsInRoleAsync(CurrentUser.Id, "Admin"))
            {
                model.AssignedTo = await UserManager.FindByIdAsync(model.AssignedToID);
            }
            else
            {
                if (ticket.CreatedByID != CurrentUser.Id)
                {
                    TempData["Fail"] = "You can't modify ticket which you didn't create!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.Remove("AssignedToID");
                    model.AssignedToID = ticket.AssignedToID;
                    model.AssignedTo = ticket.AssignedTo;

                    ModelState.Remove("Status");
                    model.Status = ticket.Status;

                    ModelState.Remove("Solution");
                    model.Solution = ticket.Solution;
                }
            }

            try
            {
                if (ModelState.IsValid)
                {
                    ticket.RequestedByID = model.RequestedByID;
                    ticket.CategoryID = model.CategoryID;
                    ticket.Title = model.Title;
                    ticket.Content = model.Content;                    
                    ticket.Status = model.Status;
                    ticket.AssignedToID = model.AssignedToID;
                    ticket.Solution = model.Solution;
                    unitOfWork.TicketRepository.Update(ticket);
                    unitOfWork.Save();
                    TempData["Success"] = "Successfully edited ticket!";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                ModelState.AddModelError("", "Cannot edit ticket. Try again!");
            }
            model.CreatedBy = ticket.CreatedBy;
            model.CreatedOn = ticket.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss");

            string adminRoleId = RoleManager.FindByName("Admin").Id;
            model.Admins = UserManager.Users.Where(u => u.Roles.FirstOrDefault(x => x.RoleId == adminRoleId) != null).OrderBy(u => u.FirstName);
            model.Categories = unitOfWork.CategoryRepository.Get(orderBy: x => x.OrderBy(c => c.Order));
            return View(model);
        }

        [OverrideAuthorization]
        public async Task<ActionResult> History(int id)
        {

            try
            {
                Ticket ticket = unitOfWork.TicketRepository.GetById(id);
                if (ticket == null)
                    throw new Exception($"Ticket id {id} doesn't exist");

                if (!await UserManager.IsInRoleAsync(CurrentUser.Id, "Admin") && ticket.CreatedByID != CurrentUser.Id && ticket.RequestedByID != CurrentUser.Id)
                    return new HttpUnauthorizedResult();

                TicketsHistoryViewModel model = new TicketsHistoryViewModel
                {
                    TicketID = id.ToString(),
                    Logs = new List<Log>()
                };
                foreach (var log in unitOfWork.TicketsHistoryRepository.Get(filters: new Expression<Func<TicketsHistory, bool>>[] { l => l.TicketId == ticket.TicketID.ToString() }, orderBy: x => x.OrderByDescending(l => l.ChangeDate)))
                {
                    AppUser changeAuthor = UserManager.FindById(log.ChangeAuthorId);
                    string logContent = String.Format("User [{0}] with ID [{1}] ", changeAuthor != null ? changeAuthor.FirstName + " " + changeAuthor.LastName : "deleted user", log.ChangeAuthorId);
                    switch (log.ActionType)
                    {
                        case "UPDATE":
                            logContent += $"changed [{log.ColumnName}] from [{log.OldValue}] to [{log.NewValue}]";
                            break;
                        case "INSERT":
                            logContent += "created ticket";
                            break;
                        case "DELETE":
                            logContent += "deleted ticket";
                            break;
                    }
                    model.Logs.Add(new Log
                    {
                        Date = log.ChangeDate,
                        Content = logContent
                    }
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
        public async Task<ActionResult> Delete(int id)
        {
            Ticket ticket = unitOfWork.TicketRepository.GetById(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            if (!(await UserManager.IsInRoleAsync(CurrentUser.Id, "Admin")) && ticket.CreatedByID != CurrentUser.Id)
                return RedirectToAction("Index");
            try
            { 
                unitOfWork.TicketRepository.Delete(id);
                unitOfWork.Save();
                TempData["Success"] = "Successfully deleted ticket!";
            }
            catch
            {
                TempData["Fail"] = "Cannot delete ticket. Try again!";
            }
            return RedirectToAction("Index");
        }

        // todo: move to UsersController or use Web Api instead if possible
        [OverrideAuthorization]
        public JsonResult FindUsers(string search)
        {
            var result = from x in UserManager.Users.Where
                         (
                              u => (u.FirstName + " " + u.LastName).ToLower().Contains(search.ToLower()) ||
                              u.Email.ToLower().Contains(search.ToLower())
                         )
                         select new
                         {
                             UserID = x.Id,
                             FirstName = x.FirstName,
                             LastName = x.LastName,
                             Email = x.Email
                         };
            return Json(result);
        }

        public FileResult DownloadTicketsAsCSV()
        {
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Created on;Created by;Requested by;Assigned to;Status;Category;Title;Content;Solution");
            foreach (Ticket ticket in unitOfWork.TicketRepository.Get(orderBy: x => x.OrderByDescending(t => t.CreatedOn)))
            {
                csv.AppendLine($"{ticket.CreatedOn};{ticket.CreatedBy?.FirstName} {ticket.CreatedBy?.LastName};{ticket.RequestedBy?.FirstName} {ticket.RequestedBy?.LastName};{ticket.AssignedTo?.FirstName} {ticket.AssignedTo?.LastName};{ticket.Status};{ticket.Category?.Name};{ticket.Title};{ticket.Content};{ticket.Solution};");
            }
            return File(Encoding.GetEncoding("ISO-8859-2").GetBytes(csv.ToString()), "text/plain", string.Format("tickets.csv"));
        }

        [HttpPost]
        public async Task<JsonResult> AssignUserToTicket(string userId, int ticketId)
        {
            try
            {
                AppUser user = await UserManager.FindByIdAsync(userId);
                Ticket ticket = unitOfWork.TicketRepository.GetById(ticketId);
                ticket.AssignedToID = user.Id;
                ticket.Status = "In progress";
                unitOfWork.TicketRepository.Update(ticket);
                unitOfWork.Save();
                TempData["Success"] = "Successfully assigned user to ticket!";
            }
            catch
            {
                TempData["Fail"] = "Cannot assign user to ticket. Try again!";
            }
            return Json(new { });
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<RedirectResult> AssignUserToTicket(string assignUserID, int assignTicketID, string returnUrl)
        //{
        //    try
        //    {
        //        AppUser user = await userManager.FindByIdAsync(assignUserID);//unitOfWork.UserRepository.GetById(assignUserID);
        //        Ticket ticket = unitOfWork.TicketRepository.GetById(assignTicketID);
        //        ticket.AssignedToID = user.Id;
        //        ticket.Status = "In progress";
        //        unitOfWork.TicketRepository.Update(ticket);
        //        unitOfWork.Save();
        //        TempData["Success"] = "Successfully assigned user to ticket!";
        //    }
        //    catch
        //    {
        //        TempData["Fail"] = "Cannot assign user to ticket. Try again!";
        //    }
        //    return Redirect(returnUrl);
        //}

        [HttpPost]
        public async Task<JsonResult> SolveTicket(string userId, int ticketId, string solution)
        {
            try
            {
                AppUser user = await UserManager.FindByIdAsync(userId);//unitOfWork.UserRepository.GetById(solveUserID);
                Ticket ticket = unitOfWork.TicketRepository.GetById(ticketId);
                ticket.AssignedToID = user.Id;
                ticket.Status = "Solved";
                ticket.Solution = solution;
                unitOfWork.TicketRepository.Update(ticket);
                unitOfWork.Save();
                TempData["Success"] = "Successfully solved ticket!";
            }
            catch
            {
                TempData["Fail"] = "Cannot solve ticket. Try again!";
            }
            return Json(new { });
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<RedirectResult> SolveTicket(string solveUserID, int solveTicketID, string solution, string returnUrl)
        //{
        //    try
        //    { 
        //        AppUser user = await userManager.FindByIdAsync(solveUserID);//unitOfWork.UserRepository.GetById(solveUserID);
        //        Ticket ticket = unitOfWork.TicketRepository.GetById(solveTicketID);
        //        ticket.AssignedToID = user.Id;
        //        ticket.Status = "Solved";
        //        ticket.Solution = solution;
        //        unitOfWork.TicketRepository.Update(ticket);
        //        unitOfWork.Save();
        //        TempData["Success"] = "Successfully solved ticket!";
        //    }
        //    catch
        //    {
        //        TempData["Fail"] = "Cannot solve ticket. Try again!";
        //    }
        //    return Redirect(returnUrl);
        //}

        [HttpPost]
        public async Task<JsonResult> CloseTicket(int ticketId)
        {
            try
            {
                AppUser user = await UserManager.FindByEmailAsync(User.Identity.Name);
                Ticket ticket = unitOfWork.TicketRepository.GetById(ticketId);
                ticket.AssignedToID = user.Id;
                ticket.Status = "Closed";
                unitOfWork.TicketRepository.Update(ticket);
                unitOfWork.Save();
                TempData["Success"] = "Successfully closed ticket!";
            }
            catch
            {
                TempData["Fail"] = "Cannot close ticket. Try again!";
            }
            return Json(new { });
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<RedirectResult> CloseTicket(int closeTicketID, string returnUrl)
        //{
        //    try
        //    { 
        //        AppUser user = await userManager.FindByEmailAsync(User.Identity.Name);
        //        Ticket ticket = unitOfWork.TicketRepository.GetById(closeTicketID);
        //        ticket.AssignedToID = user.Id;
        //        ticket.Status = "Closed";
        //        unitOfWork.TicketRepository.Update(ticket);
        //        unitOfWork.Save();
        //        TempData["Success"] = "Successfully closed ticket!";
        //    }
        //    catch
        //    {
        //        TempData["Fail"] = "Cannot close ticket. Try again!";
        //    }
        //    return Redirect(returnUrl);
        //}
    }
}

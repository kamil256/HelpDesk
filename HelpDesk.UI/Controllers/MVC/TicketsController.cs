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
using HelpDesk.UI.ViewModels.Tickets;

namespace HelpDesk.UI.Controllers.MVC
{
    [Authorize(Roles = "Admin")]
    public class TicketsController : Controller
    {
        private UserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<UserManager>();
            }
        }

        private RoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<RoleManager>();
            }
        }

        private User CurrentUser
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
            CreateViewModel model = new CreateViewModel
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
        public async Task<ActionResult> Create([Bind(Include = "RequestedByID,CategoryID,Title,Content")] CreateViewModel model)
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

                    TicketsHistory ticketHistory = new TicketsHistory
                    {
                        Date = DateTime.Now,
                        AuthorId = CurrentUser.Id,
                        TicketId = ticket.TicketID,
                        Action = "INSERT"
                    };
                    unitOfWork.TicketsHistoryRepository.Insert(ticketHistory);
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
            EditViewModel model = new EditViewModel
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

        private void saveTicketHistory(Ticket oldTicket, EditViewModel model)
        {
            if (oldTicket.RequestedByID != model.RequestedByID)
            {
                TicketsHistory ticketHistory = new TicketsHistory
                {
                    Date = DateTime.Now,
                    AuthorId = CurrentUser.Id,
                    TicketId = oldTicket.TicketID,
                    Action = "UPDATE",
                    Column = "RequestedBy",
                    OldValue = oldTicket.RequestedByID,
                    NewValue = model.RequestedByID
                };
                unitOfWork.TicketsHistoryRepository.Insert(ticketHistory);
            }

            if (oldTicket.CategoryID != model.CategoryID)
            {
                TicketsHistory ticketHistory = new TicketsHistory
                {
                    Date = DateTime.Now,
                    AuthorId = CurrentUser.Id,
                    TicketId = oldTicket.TicketID,
                    Action = "UPDATE",
                    Column = "CategoryID",
                    OldValue = oldTicket.CategoryID.ToString(),
                    NewValue = model.CategoryID.ToString()
                };
                unitOfWork.TicketsHistoryRepository.Insert(ticketHistory);
            }

            if (oldTicket.Title != model.Title)
            {
                TicketsHistory ticketHistory = new TicketsHistory
                {
                    Date = DateTime.Now,
                    AuthorId = CurrentUser.Id,
                    TicketId = oldTicket.TicketID,
                    Action = "UPDATE",
                    Column = "Title",
                    OldValue = oldTicket.Title,
                    NewValue = model.Title
                };
                unitOfWork.TicketsHistoryRepository.Insert(ticketHistory);
            }

            if (oldTicket.Content != model.Content)
            {
                TicketsHistory ticketHistory = new TicketsHistory
                {
                    Date = DateTime.Now,
                    AuthorId = CurrentUser.Id,
                    TicketId = oldTicket.TicketID,
                    Action = "UPDATE",
                    Column = "Content",
                    OldValue = oldTicket.Content,
                    NewValue = model.Content
                };
                unitOfWork.TicketsHistoryRepository.Insert(ticketHistory);
            }

            if (oldTicket.Status != model.Status)
            {
                TicketsHistory ticketHistory = new TicketsHistory
                {
                    Date = DateTime.Now,
                    AuthorId = CurrentUser.Id,
                    TicketId = oldTicket.TicketID,
                    Action = "UPDATE",
                    Column = "Status",
                    OldValue = oldTicket.Status,
                    NewValue = model.Status
                };
                unitOfWork.TicketsHistoryRepository.Insert(ticketHistory);
            }

            if (oldTicket.AssignedToID != model.AssignedToID)
            {
                TicketsHistory ticketHistory = new TicketsHistory
                {
                    Date = DateTime.Now,
                    AuthorId = CurrentUser.Id,
                    TicketId = oldTicket.TicketID,
                    Action = "UPDATE",
                    Column = "AssignedToID",
                    OldValue = oldTicket.AssignedToID,
                    NewValue = model.AssignedToID
                };
                unitOfWork.TicketsHistoryRepository.Insert(ticketHistory);
            }

            if (oldTicket.Solution != model.Solution)
            {
                TicketsHistory ticketHistory = new TicketsHistory
                {
                    Date = DateTime.Now,
                    AuthorId = CurrentUser.Id,
                    TicketId = oldTicket.TicketID,
                    Action = "UPDATE",
                    Column = "Solution",
                    OldValue = oldTicket.Solution,
                    NewValue = model.Solution
                };
                unitOfWork.TicketsHistoryRepository.Insert(ticketHistory);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OverrideAuthorization]
        public async Task<ActionResult> Edit([Bind(Include = "TicketID,RequestedByID,AssignedToID,Status,CategoryID,Title,Content,Solution")] EditViewModel model)
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
                    saveTicketHistory(ticket, model);

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

                HistoryViewModel model = new HistoryViewModel
                {
                    TicketID = id.ToString(),
                    Logs = new List<Log>()
                };
                foreach (var log in unitOfWork.TicketsHistoryRepository.Get(filters: new Expression<Func<TicketsHistory, bool>>[] { l => l.TicketId == ticket.TicketID }, orderBy: x => x.OrderByDescending(l => l.Date)))
                {
                    User changeAuthor = UserManager.FindById(log.AuthorId);
                    string logContent = String.Format("User [{0}] with ID [{1}] ", changeAuthor != null ? changeAuthor.FirstName + " " + changeAuthor.LastName : "deleted user", log.AuthorId);
                    switch (log.Action)
                    {
                        case "UPDATE":
                            logContent += $"changed [{log.Column}] from [{log.OldValue}] to [{log.NewValue}]";
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
                        Date = log.Date,
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

                TicketsHistory ticketHistory = new TicketsHistory
                {
                    Date = DateTime.Now,
                    AuthorId = CurrentUser.Id,
                    TicketId = ticket.TicketID,
                    Action = "DELETE"
                };
                unitOfWork.TicketsHistoryRepository.Insert(ticketHistory);
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
                User user = await UserManager.FindByIdAsync(userId);
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

        [HttpPost]
        public async Task<JsonResult> SolveTicket(string userId, int ticketId, string solution)
        {
            try
            {
                User user = await UserManager.FindByIdAsync(userId);
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

        [HttpPost]
        public async Task<JsonResult> CloseTicket(int ticketId)
        {
            try
            {
                User user = await UserManager.FindByEmailAsync(User.Identity.Name);
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
    }
}

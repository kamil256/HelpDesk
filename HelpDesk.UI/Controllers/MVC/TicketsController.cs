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

        private void updateTicketHistory(Ticket currentTicket, Ticket updatedTicket)
        {
            List<TicketsHistory> ticketsHistoryList = new List<TicketsHistory>();

            if (currentTicket.RequestedByID != updatedTicket.RequestedByID)
            {
                string requesterName = updatedTicket.RequestedBy != null ? $"{updatedTicket.RequestedBy.FirstName} {updatedTicket.RequestedBy.LastName}" : "";
                ticketsHistoryList.Add(new TicketsHistory { Column = "requester", NewValue = requesterName });
            }

            if (currentTicket.AssignedToID != updatedTicket.AssignedToID)
            {
                string assignedUserName = updatedTicket.AssignedTo != null ? $"{updatedTicket.AssignedTo.FirstName} {updatedTicket.AssignedTo.LastName}" : "";
                ticketsHistoryList.Add(new TicketsHistory { Column = "assigned user", NewValue = assignedUserName });
            }

            if (currentTicket.Status != updatedTicket.Status)
                ticketsHistoryList.Add(new TicketsHistory { Column = "status", NewValue = updatedTicket.Status });

            if (currentTicket.CategoryID != updatedTicket.CategoryID)
            {
                string categoryName = updatedTicket.Category != null ? updatedTicket.Category.Name : "";
                ticketsHistoryList.Add(new TicketsHistory { Column = "category", NewValue = categoryName });
            }

            if (currentTicket.Title != updatedTicket.Title)
                ticketsHistoryList.Add(new TicketsHistory { Column = "title", NewValue = updatedTicket.Title });

            if (currentTicket.Content != updatedTicket.Content)
                ticketsHistoryList.Add(new TicketsHistory { Column = "content", NewValue = updatedTicket.Content });
            
            if (currentTicket.Solution != updatedTicket.Solution)
                ticketsHistoryList.Add(new TicketsHistory { Column = "solution", NewValue = updatedTicket.Solution });

            foreach (var log in ticketsHistoryList)
            {
                log.Date = DateTime.Now;
                log.AuthorId = CurrentUser.Id;
                log.TicketId = currentTicket.TicketID;
                unitOfWork.TicketsHistoryRepository.Insert(log);
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

            //try
            {
                if (ModelState.IsValid)
                {
                    Ticket oldTicket = (Ticket)ticket.Clone();

                    ticket.RequestedByID = model.RequestedByID;
                    ticket.CategoryID = model.CategoryID;
                    ticket.Title = model.Title;
                    ticket.Content = model.Content;
                    ticket.Status = model.Status;
                    ticket.AssignedToID = model.AssignedToID;
                    ticket.Solution = model.Solution;
                    unitOfWork.TicketRepository.Update(ticket);

                    updateTicketHistory(oldTicket, ticket);
                    unitOfWork.Save();

                    TempData["Success"] = "Successfully edited ticket!";
                    return RedirectToAction("Index");
                }
            }
            //catch
            //{
            //    ModelState.AddModelError("", "Cannot edit ticket. Try again!");
            //}
            model.CreatedBy = ticket.CreatedBy;
            model.CreatedOn = ticket.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss");

            string adminRoleId = RoleManager.FindByName("Admin").Id;
            model.Admins = UserManager.Users.Where(u => u.Roles.FirstOrDefault(x => x.RoleId == adminRoleId) != null).OrderBy(u => u.FirstName);
            model.Categories = unitOfWork.CategoryRepository.Get(orderBy: x => x.OrderBy(c => c.Order));
            ViewBag.IsCurrentUserAdmin = await UserManager.IsInRoleAsync(CurrentUser.Id, "Admin");
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
                    TicketID = id,
                    Logs = new List<HistoryViewModel.Log>()
                };
                foreach (var log in unitOfWork.TicketsHistoryRepository.Get(filters: new Expression<Func<TicketsHistory, bool>>[] { l => l.TicketId == ticket.TicketID }, orderBy: x => x.OrderByDescending(l => l.Date)))
                {
                    User author = UserManager.FindById(log.AuthorId);
                    model.Logs.Add(new HistoryViewModel.Log
                    {
                        Date = log.Date,
                        AuthorId = log.AuthorId,
                        AuthorName = author != null ? $"{author.FirstName} {author.LastName}" : null,
                        Column = log.Column,
                        NewValue = log.NewValue
                    });
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
                foreach (var log in unitOfWork.TicketsHistoryRepository.Get(filters: new Expression<Func<TicketsHistory, bool>>[] { x => x.TicketId == id }))
                    unitOfWork.TicketsHistoryRepository.Delete(log);
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
                User user = await UserManager.FindByIdAsync(userId);
                Ticket ticket = unitOfWork.TicketRepository.GetById(ticketId);
                Ticket oldTicket = (Ticket)ticket.Clone();

                ticket.AssignedToID = user.Id;
                ticket.Status = "In progress";
                unitOfWork.TicketRepository.Update(ticket);

                updateTicketHistory(oldTicket, ticket);
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
                Ticket oldTicket = (Ticket)ticket.Clone();

                ticket.AssignedToID = user.Id;
                ticket.Status = "Solved";
                ticket.Solution = solution;
                unitOfWork.TicketRepository.Update(ticket);

                updateTicketHistory(oldTicket, ticket);                
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
        public JsonResult CloseTicket(int ticketId)
        {
            try
            {
                Ticket ticket = unitOfWork.TicketRepository.GetById(ticketId);
                Ticket oldTicket = (Ticket)ticket.Clone();
                
                ticket.AssignedToID = CurrentUser.Id;
                ticket.Status = "Closed";
                unitOfWork.TicketRepository.Update(ticket);

                updateTicketHistory(oldTicket, ticket);                
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

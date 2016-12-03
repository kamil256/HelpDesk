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
using HelpDesk.UI.Infrastructure;

namespace HelpDesk.UI.Controllers.MVC
{
    [Authorize(Roles = "Admin")]
    public class TicketsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IdentityHelper identityHelper;

        public TicketsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            this.identityHelper = new IdentityHelper();
        }

        [OverrideAuthorization]
        [Authorize]
        public ViewResult Index()
        {
            return View();
        }

        [OverrideAuthorization]
        [Authorize]
        public ViewResult Create()
        {
            CreateViewModel model = new CreateViewModel
            {
                Requester = identityHelper.CurrentUser,
                Categories = unitOfWork.CategoryRepository.Get(orderBy: q => q.OrderBy(c => c.Order))
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OverrideAuthorization]
        [Authorize]
        public async Task<ActionResult> Create([Bind(Include = "RequesterId,CategoryId,Title,Content")] CreateViewModel model)
        {
            if (model.RequesterId != null && await identityHelper.UserManager.FindByIdAsync(model.RequesterId) == null)
                ModelState.AddModelError("RequesterId", "Selected user doesn't exist.");
            if (model.CategoryId != null && unitOfWork.CategoryRepository.GetById((int)model.CategoryId) == null)
                ModelState.AddModelError("CategoryId", "Selected category doesn't exist.");

            try
            {
                if (ModelState.IsValid)
                {
                    Ticket ticket = new Ticket
                    {
                        CreateDate = DateTime.Now,
                        CreatorId = identityHelper.CurrentUser.Id,
                        RequesterId = model.RequesterId,
                        Status = "New",
                        CategoryId = model.CategoryId,
                        Title = model.Title,
                        Content = model.Content
                    };
                    unitOfWork.TicketRepository.Insert(ticket);
                    unitOfWork.Save();
                    TempData["Success"] = "Successfully created new ticket.";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                TempData["Fail"] = "Unable to create new ticket. Try again, and if the problem persists contact your system administrator.";
            }

            model.Requester = await identityHelper.UserManager.FindByIdAsync(model.RequesterId);
            model.Categories = unitOfWork.CategoryRepository.Get(orderBy: q => q.OrderBy(c => c.Order));
            return View(model);
        }

        [OverrideAuthorization]
        [Authorize]
        public ActionResult Edit(int id = 0)
        {
            Ticket ticket = unitOfWork.TicketRepository.GetById(id);
            if (ticket == null)
            {
                TempData["Fail"] = "Unable to display ticket details. Try again, and if the problem persists contact your system administrator.";
                return RedirectToAction("Index");
            }
            if (!identityHelper.IsCurrentUserAnAdministrator() && ticket.CreatorId != identityHelper.CurrentUser.Id && ticket.RequesterId != identityHelper.CurrentUser.Id)
            {
                TempData["Fail"] = "You can't display details of ticket which you didn't create or request.";
                return RedirectToAction("Index");
            }
            EditViewModel model = new EditViewModel
            {
                TicketId = ticket.TicketId,
                CreateDate = ticket.CreateDate.ToString("yyyy-MM-dd hh:mm:ss"),
                RequesterId = ticket.RequesterId,
                AssignedUserId = ticket.AssignedUserId,
                Status = ticket.Status,
                CategoryId = ticket.CategoryId,
                Title = ticket.Title,
                Content = ticket.Content,
                Solution = ticket.Solution,
                Creator = ticket.Creator,
                Requester = ticket.Requester,
                AssignedUser = ticket.AssignedUser
            };
            string adminRoleId = identityHelper.RoleManager.FindByName("Admin").Id;
            model.Administrators = identityHelper.UserManager.Users.Where(u => u.Roles.FirstOrDefault(r => r.RoleId == adminRoleId) != null).OrderBy(u => u.FirstName);
            model.Categories = unitOfWork.CategoryRepository.Get(orderBy: q => q.OrderBy(c => c.Order));
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OverrideAuthorization]
        [Authorize]
        public async Task<ActionResult> Edit([Bind(Include = "TicketId,RequesterId,AssignedUserId,Status,CategoryId,Title,Content,Solution")] EditViewModel model)
        {
            Ticket ticket = unitOfWork.TicketRepository.GetById(model.TicketId);
            if (ticket == null)
            {
                TempData["Fail"] = "Unable to edit ticket. Try again, and if the problem persists contact your system administrator.";
                return RedirectToAction("Index");
            }
            if (!identityHelper.IsCurrentUserAnAdministrator() && ticket.CreatorId != identityHelper.CurrentUser.Id && ticket.RequesterId != identityHelper.CurrentUser.Id)
            {
                TempData["Fail"] = "You can't display details of ticket which you didn't create or request.";
                return RedirectToAction("Index");
            }

            if (model.RequesterId != null && await identityHelper.UserManager.FindByIdAsync(model.RequesterId) == null)
                ModelState.AddModelError("RequesterId", "Selected user doesn't exist.");
            if (model.AssignedUserId != null && await identityHelper.UserManager.FindByIdAsync(model.AssignedUserId) == null)
                ModelState.AddModelError("AssignedUserId", "Selected user doesn't exist.");
            if (model.Status != "New" && model.Status != "In progress" && model.Status != "Solved" && model.Status != "Closed")
                ModelState.AddModelError("Status", $"Status \"{model.Status}\" is incorrect.");
            if (model.CategoryId != null && unitOfWork.CategoryRepository.GetById((int)model.CategoryId) == null)
                ModelState.AddModelError("CategoryId", "Selected category doesn't exist.");

            
            if (!identityHelper.IsCurrentUserAnAdministrator())
            {
                ModelState.Remove("AssignedUserId");
                model.AssignedUserId = ticket.AssignedUserId;
                model.AssignedUser = ticket.AssignedUser;

                ModelState.Remove("Status");
                model.Status = ticket.Status;

                ModelState.Remove("Solution");
                model.Solution = ticket.Solution;
            }

            try
            {
                if (ModelState.IsValid)
                {
                    Ticket oldTicket = (Ticket)ticket.Clone();

                    ticket.RequesterId = model.RequesterId;
                    ticket.AssignedUserId = model.AssignedUserId;
                    ticket.Status = model.Status;
                    ticket.CategoryId = model.CategoryId;
                    ticket.Title = model.Title;
                    ticket.Content = model.Content;
                    ticket.Solution = model.Solution;
                    unitOfWork.TicketRepository.Update(ticket);

                    updateTicketHistory(oldTicket, ticket);
                    unitOfWork.Save();
                    TempData["Success"] = "Successfully edited ticket.";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                TempData["Fail"] = "Unable to edit ticket. Try again, and if the problem persists contact your system administrator.";
            }
            model.Creator = ticket.Creator;
            model.CreateDate = ticket.CreateDate.ToString("yyyy-MM-dd hh:mm:ss");
            model.Requester = await identityHelper.UserManager.FindByIdAsync(model.RequesterId);
            model.AssignedUser = await identityHelper.UserManager.FindByIdAsync(model.AssignedUserId);

            string adminRoleId = identityHelper.RoleManager.FindByName("Admin").Id;
            model.Administrators = identityHelper.UserManager.Users.Where(u => u.Roles.FirstOrDefault(r => r.RoleId == adminRoleId) != null).OrderBy(u => u.FirstName);
            model.Categories = unitOfWork.CategoryRepository.Get(orderBy: q => q.OrderBy(c => c.Order));
            return View(model);
        }

        [OverrideAuthorization]
        [Authorize]
        public ActionResult History(int id = 0)
        {
            Ticket ticket = unitOfWork.TicketRepository.GetById(id);
            if (ticket == null)
            {
                TempData["Fail"] = "Unable to display ticket history. Try again, and if the problem persists contact your system administrator.";
                return RedirectToAction("Index");
            }
            if (!identityHelper.IsCurrentUserAnAdministrator() && ticket.CreatorId != identityHelper.CurrentUser.Id && ticket.RequesterId != identityHelper.CurrentUser.Id)
            {
                TempData["Fail"] = "You can't display history of ticket which you didn't create or request.";
                return RedirectToAction("Index");
            }

            HistoryViewModel model = new HistoryViewModel
            {
                TicketId = id,
                Logs = new List<HistoryViewModel.Log>()
            };
            foreach (var log in unitOfWork.TicketsHistoryRepository.Get(filters: new Expression<Func<TicketsHistory, bool>>[] { l => l.TicketId == ticket.TicketId }, orderBy: q => q.OrderByDescending(l => l.Date)))
            {
                User author = identityHelper.UserManager.FindById(log.AuthorId);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int ticketId = 0)
        {
            Ticket ticket = unitOfWork.TicketRepository.GetById(ticketId);
            if (ticket == null)
            {
                TempData["Fail"] = "Unable to delete ticket. Try again, and if the problem persists contact your system administrator.";
                return RedirectToAction("Index");
            }

            try
            {
                foreach (var log in unitOfWork.TicketsHistoryRepository.Get(filters: new Expression<Func<TicketsHistory, bool>>[] { t => t.TicketId == ticketId }))
                    unitOfWork.TicketsHistoryRepository.Delete(log);
                unitOfWork.TicketRepository.Delete(ticketId);
                unitOfWork.Save();
                TempData["Success"] = "Successfully deleted ticket.";
            }
            catch
            {
                TempData["Fail"] = "Unable to delete ticket. Try again, and if the problem persists contact your system administrator.";
            }
            return RedirectToAction("Index");
        }

        public FileResult DownloadTicketsAsCSV()
        {
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Created on;Created by;Requested by;Assigned to;Status;Category;Title;Content;Solution");
            foreach (Ticket ticket in unitOfWork.TicketRepository.Get(orderBy: x => x.OrderByDescending(t => t.CreateDate)))
            {
                csv.AppendLine($"{ticket.CreateDate};{ticket.Creator?.FirstName} {ticket.Creator?.LastName};{ticket.Requester?.FirstName} {ticket.Requester?.LastName};{ticket.AssignedUser?.FirstName} {ticket.AssignedUser?.LastName};{ticket.Status};{ticket.Category?.Name};{ticket.Title};{ticket.Content};{ticket.Solution};");
            }
            return File(Encoding.GetEncoding("ISO-8859-2").GetBytes(csv.ToString()), "text/plain", string.Format("tickets.csv"));
        }

        [HttpPost]
        public async Task<JsonResult> AssignUserToTicket(string userId, int ticketId)
        {
            User user = await identityHelper.UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new
                {
                    Fail = "Cannot assign ticket to user which doesn't exist. Try again, and if the problem persists contact your system administrator."
                });
            }

            Ticket ticket = unitOfWork.TicketRepository.GetById(ticketId);
            if (ticket == null)
            {
                return Json(new
                {
                    Fail = "Cannot assign user to ticket which doesn't exist. Try again, and if the problem persists contact your system administrator."
                });
            }

            try
            {
                Ticket oldTicket = (Ticket)ticket.Clone();

                ticket.AssignedUserId = user.Id;
                ticket.Status = "In progress";
                unitOfWork.TicketRepository.Update(ticket);

                updateTicketHistory(oldTicket, ticket);
                unitOfWork.Save();
            }
            catch
            {
                return Json(new
                {
                    Fail = "Cannot assign user to ticket. Try again, and if the problem persists contact your system administrator."
                });
            }

            return Json(new
            {
                Success = "Successfully assigned user to ticket."
            });
        }

        [HttpPost]
        public async Task<JsonResult> SolveTicket(string userId, int ticketId, string solution)
        {
            User user = await identityHelper.UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new
                {
                    Fail = "Cannot solve ticket by user which doesn't exist. Try again, and if the problem persists contact your system administrator."
                });
            }

            Ticket ticket = unitOfWork.TicketRepository.GetById(ticketId);
            if (ticket == null)
            {
                return Json(new
                {
                    Fail = "Cannot solve ticket which doesn't exist. Try again, and if the problem persists contact your system administrator."
                });
            }

            try
            {
                Ticket oldTicket = (Ticket)ticket.Clone();

                ticket.AssignedUserId = user.Id;
                ticket.Status = "Solved";
                ticket.Solution = solution;
                unitOfWork.TicketRepository.Update(ticket);

                updateTicketHistory(oldTicket, ticket);
                unitOfWork.Save();
            }
            catch
            { 
                return Json(new
                {
                    Fail = "Cannot solve ticket. Try again, and if the problem persists contact your system administrator."
                });
            }

            return Json(new
            {
                Success = "Successfully solved ticket."
            });
        }

        [HttpPost]
        public JsonResult CloseTicket(int ticketId)
        {
            Ticket ticket = unitOfWork.TicketRepository.GetById(ticketId);
            if (ticket == null)
            {
                return Json(new
                {
                    Fail = "Cannot close ticket which doesn't exist. Try again, and if the problem persists contact your system administrator."
                });
            }

            try
            {
                Ticket oldTicket = (Ticket)ticket.Clone();
                
                ticket.AssignedUserId = identityHelper.CurrentUser.Id;
                ticket.Status = "Closed";
                unitOfWork.TicketRepository.Update(ticket);

                updateTicketHistory(oldTicket, ticket);                
                unitOfWork.Save();
            }
            catch
            {
                return Json(new
                {
                    Fail = "Cannot close ticket. Try again, and if the problem persists contact your system administrator."
                });
            }

            return Json(new
            {
                Success = "Successfully closed ticket."
            });
        }

        private void updateTicketHistory(Ticket currentTicket, Ticket updatedTicket)
        {
            List<TicketsHistory> ticketsHistoryList = new List<TicketsHistory>();

            if (currentTicket.RequesterId != updatedTicket.RequesterId)
            {
                string requesterName = updatedTicket.Requester != null ? $"{updatedTicket.Requester.FirstName} {updatedTicket.Requester.LastName}" : "";
                ticketsHistoryList.Add(new TicketsHistory { Column = "requester", NewValue = requesterName });
            }

            if (currentTicket.AssignedUserId != updatedTicket.AssignedUserId)
            {
                string assignedUserName = updatedTicket.AssignedUser != null ? $"{updatedTicket.AssignedUser.FirstName} {updatedTicket.AssignedUser.LastName}" : "";
                ticketsHistoryList.Add(new TicketsHistory { Column = "assigned user", NewValue = assignedUserName });
            }

            if (currentTicket.Status != updatedTicket.Status)
                ticketsHistoryList.Add(new TicketsHistory { Column = "status", NewValue = updatedTicket.Status });

            if (currentTicket.CategoryId != updatedTicket.CategoryId)
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
                log.AuthorId = identityHelper.CurrentUser.Id;
                log.TicketId = currentTicket.TicketId;
                unitOfWork.TicketsHistoryRepository.Insert(log);
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }
            filterContext.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.aspx"
            };
            filterContext.ExceptionHandled = true;
        }
    }
}

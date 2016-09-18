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

namespace HelpDesk.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private IUnitOfWork unitOfWork;

        public TicketsController()
        {
            unitOfWork = new UnitOfWork();
        }

        public ActionResult Index([Bind(Include = "Status,AssignedToID,CategoryID,Search,AdvancedSearch,SortBy,DescSort,Page")] TicketsIndexViewModel model)
        {
            List<Expression<Func<Ticket, bool>>> filters = new List<Expression<Func<Ticket, bool>>>();
            if (model.Status != "All")
                filters.Add(t => t.Status == model.Status);
            if (model.AssignedToID != null)
                filters.Add(t => t.AssignedToID == model.AssignedToID);
            if (model.CategoryID != null)
                filters.Add(t => t.CategoryID == model.CategoryID);
            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                if (model.AdvancedSearch)
                    filters.Add(t => t.Title.ToLower().Contains(model.Search.ToLower()) ||
                                     t.Content.ToLower().Contains(model.Search.ToLower()) ||
                                     t.Solution.ToLower().Contains(model.Search.ToLower()));
                else
                    filters.Add(t => t.Title.ToLower().Contains(model.Search.ToLower()));
            }
            
            Expression<Func<Ticket, object>> orderByPropertySelector = null;
            switch (model.SortBy)
            {
                case "CreatedOn":
                    orderByPropertySelector = t => t.CreatedOn.ToString();
                    break;
                case "RequestedBy":
                    orderByPropertySelector = t => t.RequestedBy.FirstName;
                    break;
                case "Title":
                    orderByPropertySelector = t => t.Title;
                    break;
                case "Category":
                    orderByPropertySelector = t => t.Category.Name;
                    break;
                case "Status":
                    orderByPropertySelector = t => t.Status;
                    break;
                case "AssignedTo":
                    orderByPropertySelector = t => t.AssignedTo.FirstName;
                    break;
            }
            Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>> orderBy = null;
            if (orderByPropertySelector != null)
            {
                if (model.DescSort)
                    orderBy = x => x.OrderByDescending(orderByPropertySelector);
                else
                    orderBy = x => x.OrderBy(orderByPropertySelector);
            }

            model.Admins = unitOfWork.UserRepository.GetAll(u => u.Role == "Admin", orderBy: o => o.OrderBy(t => t.FirstName));
            model.Categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order));
            model.Tickets = unitOfWork.TicketRepository.GetAll(filters: filters, orderBy: orderBy).ToPagedList(model.Page, 2);
            return View(model);
        }

        public ActionResult Create()
        {
            TicketsCreateViewModel model = new TicketsCreateViewModel();
            model.RequestedBy = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single();
            model.RequestedByID = model.RequestedBy.UserID;
            model.Categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order));
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RequestedByID,CategoryID,Title,Content")] TicketsCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Ticket ticket = new Ticket()
                    {
                        CreatedBy = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single(),
                        RequestedByID = model.RequestedByID,
                        CreatedOn = DateTime.Now,
                        Status = "New",
                        CategoryID = model.CategoryID ?? 0,
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

            model.RequestedBy = unitOfWork.UserRepository.GetById(model.RequestedByID ?? 0);
            model.Categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order));

            return View(model);
        }

        public ActionResult Edit(int id = 0)
        {
            Ticket ticket = unitOfWork.TicketRepository.GetById(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            TicketsEditViewModel model = new TicketsEditViewModel()
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

            model.Admins = unitOfWork.UserRepository.GetAll(u => u.Role == "Admin", orderBy: o => o.OrderBy(t => t.FirstName));
            model.Categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order));
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TicketID,RequestedByID,AssignedToID,Status,CategoryID,Title,Content,Solution")] TicketsEditViewModel model)
        {
            Ticket ticket = unitOfWork.TicketRepository.GetById(model.TicketID);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            model.RequestedBy = unitOfWork.UserRepository.GetById(model.RequestedByID ?? 0);
            model.AssignedTo = unitOfWork.UserRepository.GetById(model.AssignedToID ?? 0);
            try
            {
                if (ModelState.IsValid)
                {
                    ticket.RequestedByID = model.RequestedByID;
                    ticket.AssignedToID = model.AssignedToID;
                    ticket.Status = model.Status;
                    ticket.CategoryID = model.CategoryID;
                    ticket.Title = model.Title;
                    ticket.Content = model.Content;
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

            model.Admins = unitOfWork.UserRepository.GetAll(u => u.Role == "Admin", orderBy: o => o.OrderBy(t => t.FirstName));
            model.Categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order));

            return View(model);
        }

        // GET: Users/Edit/5
        public ActionResult EditPassword(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = unitOfWork.UserRepository.GetById(id ?? 0);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(new UsersChangePasswordViewModel
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName
            });
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPassword([Bind(Include = "UserID,Password,ConfirmPassword")] UsersChangePasswordViewModel user)
        {
            if (ModelState.IsValid)
            {
                User editedUser = new User();
                editedUser.UserID = user.UserID;
                editedUser.Salt = Guid.NewGuid().ToString();
                //editedUser.Password = HashPassword(user.Password, editedUser.Salt);
                //unitOfWork.UserRepository.UpdateUserPassword(editedUser);
                unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
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

        public JsonResult FindUsers(string search)
        {
            var result = from x in unitOfWork.UserRepository.GetAll
                         (
                              u => (u.FirstName + " " + u.LastName).ToLower().Contains(search.ToLower()) ||
                              u.Email.ToLower().Contains(search.ToLower())
                         )
                         select new
                         {
                             UserID = x.UserID,
                             FirstName = x.FirstName,
                             LastName = x.LastName,
                             Email = x.Email
                         };
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public RedirectResult AssignUserToTicket(int assignUserID, int assignTicketID, string returnUrl)
        {
            try
            {
                User user = unitOfWork.UserRepository.GetById(assignUserID);
                Ticket ticket = unitOfWork.TicketRepository.GetById(assignTicketID);
                ticket.AssignedTo = user;
                ticket.Status = "In progress";
                unitOfWork.TicketRepository.Update(ticket);
                unitOfWork.Save();
                TempData["Success"] = "Successfully assigned user to ticket!";
            }
            catch
            {
                TempData["Fail"] = "Cannot assign user to ticket. Try again!";
            }
            return Redirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public RedirectResult SolveTicket(int solveUserID, int solveTicketID, string solution, string returnUrl)
        {
            try
            { 
                User user = unitOfWork.UserRepository.GetById(solveUserID);
                Ticket ticket = unitOfWork.TicketRepository.GetById(solveTicketID);
                ticket.AssignedTo = user;
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
            return Redirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public RedirectResult CloseTicket(int closeTicketID, string returnUrl)
        {
            try
            { 
                User user = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single();
                Ticket ticket = unitOfWork.TicketRepository.GetById(closeTicketID);
                ticket.AssignedTo = user;
                ticket.Status = "Closed";
                unitOfWork.TicketRepository.Update(ticket);
                unitOfWork.Save();
                TempData["Success"] = "Successfully closed ticket!";
            }
            catch
            {
                TempData["Fail"] = "Cannot close ticket. Try again!";
            }
            return Redirect(returnUrl);
        }
    }
}

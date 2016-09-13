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

        public JsonResult AssignPersonToTicket(int UserID, int TicketID)
        {
            User user = unitOfWork.UserRepository.GetById(UserID);
            Ticket ticket = unitOfWork.TicketRepository.GetById(TicketID);
            ticket.AssignedTo = user;
            ticket.Status = "In progress";
            unitOfWork.TicketRepository.Update(ticket);
            unitOfWork.Save();
            return Json(new { success = true });
        }

        public ActionResult Index([Bind(Include = "Status,AssignedTo,Category,Search,SortBy,DescSort,Page")] TicketsIndexViewModel model)
        {
            List<Expression<Func<Ticket, bool>>> filters = new List<Expression<Func<Ticket, bool>>>();
            if (model.Status != "All")
                filters.Add(t => t.Status == model.Status);
            if (model.AssignedTo != 0)
                filters.Add(t => t.AssignedToID == model.AssignedTo);
            if (model.Category != 0)
                filters.Add(t => t.CategoryID == model.Category);
            if (!string.IsNullOrWhiteSpace(model.Search))
                filters.Add(t => t.Title.ToLower().Contains(model.Search.ToLower()) ||
                                 t.Content.ToLower().Contains(model.Search.ToLower()) ||
                                 t.Solution.ToLower().Contains(model.Search.ToLower()));
            
            Expression<Func<Ticket, object>> orderByPropertySelector = null;
            switch (model.SortBy)
            {
                case "CreatedOn":
                    orderByPropertySelector = t => t.CreateDate.ToString();
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
            
            List<Category> categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order)).ToList();
            categories.Insert(0, new Category() { CategoryID = 0, Name = "All" });
            model.Categories = new SelectList
            (
                items: categories, 
                dataValueField: "CategoryID", 
                dataTextField: "Name", 
                selectedValue: model.Category
            );

            model.Statuses = new SelectList(new string[] { "All", "New", "In progress", "Solved", "Closed" }, model.Status);

            var admins = (from u in unitOfWork.UserRepository.GetAll(u => u.Role == "Admin", orderBy: o => o.OrderBy(t => t.FirstName))
                          select new { value = u.UserID, name = $"{u.FirstName} {u.LastName}" }).ToList();
            admins.Insert(0, new { value = 0, name = "Anybody" });
            model.AdminsList = new SelectList
            (
                items: admins, 
                dataValueField: "value", 
                dataTextField: "name", 
                selectedValue: model.AssignedTo
            );

            model.Tickets = unitOfWork.TicketRepository.GetAll(filters: filters, orderBy: orderBy).ToPagedList(model.Page, 2);
            return View(model);
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
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
            return View(user);
        }

        public ActionResult Create()
        {
            TicketsCreateViewModel model = new TicketsCreateViewModel();
            model.Requestor = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single();
            model.RequestorID = model.Requestor.UserID;
            List<Category> categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order)).ToList();
            categories.Insert(0, new Category() { CategoryID = 0, Name = "-" });
            model.Categories = new SelectList(categories, "CategoryID", "Name", 0);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RequestorID,Category,Title,Content")] TicketsCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                Ticket ticket = new Ticket()
                {
                    CreatedBy = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single(),
                    RequestedBy = unitOfWork.UserRepository.GetById(model.RequestorID),
                    Category = unitOfWork.CategoryRepository.GetAll(c => c.CategoryID == model.Category).Single(),
                    CreateDate = DateTime.Now,
                    Status = "New",
                    Title = model.Title,
                    Content = model.Content
                };
                unitOfWork.TicketRepository.Insert(ticket);
                unitOfWork.Save();
                return RedirectToAction("Index");
            }

            model.Requestor = unitOfWork.UserRepository.GetById(model.RequestorID);

            List<Category> categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order)).ToList();
            categories.Insert(0, new Category() { CategoryID = 0, Name = "-" });
            model.Categories = new SelectList(categories, "CategoryID", "Name", model.Category);

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
                Creator = ticket.CreatedBy,
                Solver = ticket.AssignedTo,
                CreateDate = ticket.CreateDate.ToShortDateString() + " " + ticket.CreateDate.ToShortTimeString(),
                //SolveOrCloseDate = ticket.SolveOrCloseDate != null ? ticket.SolveOrCloseDate?.ToShortDateString() + " " + ticket.SolveOrCloseDate?.ToShortTimeString() : null,
                RequestorID = ticket.RequestedByID,
                Requestor = ticket.RequestedByID != null ? unitOfWork.UserRepository.GetById(ticket.RequestedByID ?? 0) : null,
                Category = ticket.CategoryID,
                Status = ticket.Status,
                Title = ticket.Title,
                Content = ticket.Content,
                Solution = ticket.Solution
            };

            List<Category> categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order)).ToList();
            categories.Insert(0, new Category() { CategoryID = 0, Name = "-" });
            model.Categories = new SelectList(categories, "CategoryID", "Name", model.Category);
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TicketID,RequestorID,Category,Status,Title,Content,Solution")] TicketsEditViewModel model)
        {
            model.Requestor = unitOfWork.UserRepository.GetById(model.RequestorID ?? 0);
            User solver = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single();
            Ticket ticket = unitOfWork.TicketRepository.GetById(model.TicketID);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            if (ModelState.IsValid)
            {
                ticket.RequestedByID = model.RequestorID;
                
                ticket.CategoryID = model.Category;

                string previousStatus = ticket.Status;
                // If solver doesn't change status and status remains New, but he changes solution, 
                // status changes to Solved
                if (model.Status == "New" && model.Status == previousStatus &&
                    !string.IsNullOrEmpty(model.Solution) && model.Solution != ticket.Solution)
                    ticket.Status = "Solved";
                else
                    ticket.Status = model.Status;
                if (ticket.Status == "New")
                {
                    
                    ticket.AssignedToID = null;
                }
                if ((ticket.Status == "Solved" && previousStatus != "Solved") ||
                    (ticket.Status == "Closed" && previousStatus != "Closed"))
                {
                    
                    ticket.AssignedToID = solver.UserID;
                }
                ticket.Title = model.Title;
                ticket.Content = model.Content;
                ticket.Solution = model.Solution;
                unitOfWork.TicketRepository.Update(ticket);
                unitOfWork.Save();
                return RedirectToAction("Index");
            }
            model.Creator = ticket.CreatedBy;
            model.Solver = ticket.AssignedTo;
            model.CreateDate = ticket.CreateDate.ToShortDateString() + " " + ticket.CreateDate.ToShortTimeString();
            //model.SolveOrCloseDate = ticket.SolveOrCloseDate != null ? ticket.SolveOrCloseDate?.ToShortDateString() + " " + ticket.SolveOrCloseDate?.ToShortTimeString() : null;
            

            List<Category> categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order)).ToList();
            categories.Insert(0, new Category() { CategoryID = 0, Name = "-" });
            model.Categories = new SelectList(categories, "CategoryID", "Name", model.Category);
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

        public ActionResult Delete(int id = 0)
        {
            Ticket ticket = unitOfWork.TicketRepository.GetById(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            unitOfWork.TicketRepository.Delete(id);
            unitOfWork.Save();
            return RedirectToAction("Index");
        }
    }
}

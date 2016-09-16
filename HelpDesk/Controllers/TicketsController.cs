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

        public ActionResult Index([Bind(Include = "Status,AssignedTo,Category,Search,AdvancedSearch,SortBy,DescSort,Page")] TicketsIndexViewModel model)
        {
            List<Expression<Func<Ticket, bool>>> filters = new List<Expression<Func<Ticket, bool>>>();
            if (model.Status != "All")
                filters.Add(t => t.Status == model.Status);
            if (model.AssignedTo != null)
                filters.Add(t => t.AssignedToID == model.AssignedTo);
            if (model.Category != null)
                filters.Add(t => t.CategoryID == model.Category);
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

            model.Admins = unitOfWork.UserRepository.GetAll(u => u.Role == "Admin", orderBy: o => o.OrderBy(t => t.FirstName));
            model.Categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order));
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
            model.RequestedByID = model.Requestor.UserID;
            List<Category> categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order)).ToList();
            categories.Insert(0, new Category() { CategoryID = 0, Name = "-" });
            model.Categories = new SelectList(categories, "CategoryID", "Name", 0);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RequestedByID,CategoryID,Title,Content")] TicketsCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                Ticket ticket = new Ticket()
                {
                    CreatedBy = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single(),
                    RequestedBy = unitOfWork.UserRepository.GetById(model.RequestedByID),
                    Category = unitOfWork.CategoryRepository.GetAll(c => c.CategoryID == model.CategoryID).Single(),
                    CreateDate = DateTime.Now,
                    Status = "New",
                    Title = model.Title,
                    Content = model.Content
                };
                unitOfWork.TicketRepository.Insert(ticket);
                unitOfWork.Save();
                return RedirectToAction("Index");
            }

            model.Requestor = unitOfWork.UserRepository.GetById(model.RequestedByID);

            List<Category> categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order)).ToList();
            categories.Insert(0, new Category() { CategoryID = 0, Name = "-" });
            model.Categories = new SelectList(categories, "CategoryID", "Name", model.CategoryID);

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
                CreatedBy = ticket.CreatedBy,
                RequestedByID = ticket.RequestedByID,
                RequestedBy = ticket.RequestedBy,
                AssignedToID = ticket.AssignedToID,
                AssignedTo = ticket.AssignedTo,
                CreatedOn = ticket.CreateDate.ToShortDateString() + " " + ticket.CreateDate.ToShortTimeString(),
                StatusID = ticket.Status,
                CategoryID = ticket.CategoryID,
                Title = ticket.Title,
                Content = ticket.Content,
                Solution = ticket.Solution
            };

            var categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order)).ToList();
            model.Categories = new SelectList
            (
                items: categories,
                dataValueField: "CategoryID",
                dataTextField: "Name",
                selectedValue: model.CategoryID
            );

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TicketID,RequestedByID,AssignedToID,StatusID,CategoryID,Title,Content,Solution")] TicketsEditViewModel model)
        {
            Ticket ticket = unitOfWork.TicketRepository.GetById(model.TicketID);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            model.RequestedBy = unitOfWork.UserRepository.GetById(model.RequestedByID ?? 0);
            model.AssignedTo = unitOfWork.UserRepository.GetById(model.AssignedToID ?? 0);
                        
            if (ModelState.IsValid)
            {
                string previousStatus = ticket.Status;
                int? previousAssignedToID = ticket.AssignedToID;
                ticket.RequestedByID = model.RequestedByID;
                ticket.AssignedToID = model.AssignedToID;       
                // If solver doesn't change status and status remains New or In progress, but he changes solution, 
                // status changes to Solved
                if ((model.StatusID == "New" || model.StatusID == "In progress") && model.StatusID == previousStatus &&
                    !string.IsNullOrEmpty(model.Solution) && model.Solution != ticket.Solution)
                    ticket.Status = "Solved";
                else
                    ticket.Status = model.StatusID;
                if (((ticket.Status == "In progress" && previousStatus != "In progress") ||
                     (ticket.Status == "Solved" && previousStatus != "Solved") ||
                     (ticket.Status == "Closed" && previousStatus != "Closed")) &&
                     model.AssignedToID == null)
                {
                    
                    ticket.AssignedToID = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single().UserID;
                }
                if (previousAssignedToID == null && model.AssignedToID != null)
                    ticket.Status = "In progress";
                ticket.CategoryID = model.CategoryID;
                ticket.Title = model.Title;
                ticket.Content = model.Content;
                ticket.Solution = model.Solution;
                unitOfWork.TicketRepository.Update(ticket);
                unitOfWork.Save();
                return RedirectToAction("Index");
            }
            model.CreatedBy = ticket.CreatedBy;
            model.CreatedOn = ticket.CreateDate.ToShortDateString() + " " + ticket.CreateDate.ToShortTimeString();

            var categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order)).ToList();
            model.Categories = new SelectList
            (
                items: categories,
                dataValueField: "CategoryID",
                dataTextField: "Name",
                selectedValue: model.CategoryID
            );

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
        public RedirectResult AssignPersonToTicket(int assignUserID, int assignTicketID, string returnUrl)
        {
            User user = unitOfWork.UserRepository.GetById(assignUserID);
            Ticket ticket = unitOfWork.TicketRepository.GetById(assignTicketID);
            ticket.AssignedTo = user;
            ticket.Status = "In progress";
            unitOfWork.TicketRepository.Update(ticket);
            unitOfWork.Save();
            return Redirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public RedirectResult SolveTicket(int solveUserID, int solveTicketID, string solution, string returnUrl)
        {
            User user = unitOfWork.UserRepository.GetById(solveUserID);
            Ticket ticket = unitOfWork.TicketRepository.GetById(solveTicketID);
            ticket.AssignedTo = user;
            ticket.Status = "Solved";
            ticket.Solution = solution;
            unitOfWork.TicketRepository.Update(ticket);
            unitOfWork.Save();
            return Redirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public RedirectResult CloseTicket(int closeTicketID, string returnUrl)
        {
            User user = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single();
            Ticket ticket = unitOfWork.TicketRepository.GetById(closeTicketID);
            ticket.AssignedTo = user;
            ticket.Status = "Closed";
            unitOfWork.TicketRepository.Update(ticket);
            unitOfWork.Save();
            return Redirect(returnUrl);
        }
    }
}

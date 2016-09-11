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
                             FirstName = x.FirstName,
                             LastName = x.LastName,
                             Email = x.Email
                         };
            return Json(result);
        }

        public ActionResult Index([Bind(Include = "DateFrom,DateTo,Category,Status,Search,SortBy,DescSort,Page")] TicketsIndexViewModel model)
        {
            List<Expression<Func<Ticket, bool>>> filters = new List<Expression<Func<Ticket, bool>>>();
            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                filters.Add(t => t.Title.ToLower().Contains(model.Search.ToLower()) ||
                                 t.Content.ToLower().Contains(model.Search.ToLower()) ||
                                 t.Solution.ToLower().Contains(model.Search.ToLower()));
            }
            if (model.Category != 0)
                filters.Add(t => t.CategoryID == model.Category);
            if (model.Status != "All")
                filters.Add(t => t.status == model.Status);
            if (model.DateFrom != default(DateTime))
                filters.Add(t => t.CreateDate >= model.DateFrom);
            if (model.DateTo != default(DateTime))
                filters.Add(t => t.CreateDate < DbFunctions.AddDays(model.DateTo, 1));
            Expression<Func<Ticket, object>> propertySelector = null;
            switch (model.SortBy)
            {
                case "CreateDate":
                    propertySelector = t => t.CreateDate.ToString();
                    break;
                case "Requestor":
                    propertySelector = t => t.Requestor.FirstName;
                    break;
                case "Title":
                    propertySelector = t => t.Title;
                    break;
                case "Category":
                    propertySelector = t => t.Category.Name;
                    break;
                case "Status":
                    propertySelector = t => t.status;
                    break;
            }
            Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>> orderBy = null;
            if (propertySelector != null)
            {
                if (model.DescSort)
                    orderBy = x => x.OrderByDescending(propertySelector);
                else
                    orderBy = x => x.OrderBy(propertySelector);
            }
            model.Tickets = unitOfWork.TicketRepository.GetAll(filters: filters, orderBy: orderBy).ToPagedList(model.Page, 2);

            List<Category> categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order)).ToList();
            categories.Insert(0, new Category() { CategoryID = 0, Name = "All" });
            model.Categories = new SelectList(categories, "CategoryID", "Name", model.Category);

            model.Statuses = new SelectList(new string[] { "All", "New", "Solved", "Closed" }, model.Status);

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
            User currentUser = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single();
            model.UsersList = new SelectList(new[] 
            {
                new
                {
                    display = $"{currentUser.FirstName} {currentUser.LastName} ({currentUser.Email})",
                    value = currentUser.Email
                }
            }, "value", "display", currentUser.Email);

            List<Category> categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order)).ToList();
            categories.Insert(0, new Category() { CategoryID = 0, Name = "-" });
            model.Categories = new SelectList(categories, "CategoryID", "Name", 0);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Requestor,Category,Title,Content")] TicketsCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                Ticket ticket = new Ticket()
                {
                    Creator = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single(),
                    Requestor = unitOfWork.UserRepository.GetAll(u => u.Email == model.Requestor).Single(),
                    Category = unitOfWork.CategoryRepository.GetAll(c => c.CategoryID == model.Category).Single(),
                    CreateDate = DateTime.Now,
                    status = "New",
                    Title = model.Title,
                    Content = model.Content
                };
                unitOfWork.TicketRepository.Insert(ticket);
                unitOfWork.Save();
                return RedirectToAction("Index");
            }

            User user = unitOfWork.UserRepository.GetAll(u => u.Email == model.Requestor).Single();
            model.UsersList = new SelectList(new[]
            {
                new
                {
                    display = $"{user.FirstName} {user.LastName} ({user.Email})",
                    value = user.Email
                }
            }, "value", "display", user.Email);

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
                Creator = ticket.Creator,
                Solver = ticket.Solver,
                CreateDate = ticket.CreateDate.ToShortDateString() + " " + ticket.CreateDate.ToShortTimeString(),
                SolveOrCloseDate = ticket.SolveOrCloseDate != null ? ticket.SolveOrCloseDate?.ToShortDateString() + " " + ticket.SolveOrCloseDate?.ToShortTimeString() : null,
                Requestor = ticket.Requestor?.Email,
                Category = ticket.CategoryID,
                Status = ticket.status,
                Title = ticket.Title,
                Content = ticket.Content,
                Solution = ticket.Solution
            };

            User user = unitOfWork.UserRepository.GetAll(u => u.Email == model.Requestor).SingleOrDefault();
            if (user != null)
                model.UsersList = new SelectList(new[]
                {
                    new
                    {
                        display = $"{user.FirstName} {user.LastName} ({user.Email})",
                        value = user.Email
                    }
                }, "value", "display", user.Email);
            else
                model.UsersList = new SelectList(new object[] { });
            List<Category> categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order)).ToList();
            categories.Insert(0, new Category() { CategoryID = 0, Name = "-" });
            model.Categories = new SelectList(categories, "CategoryID", "Name", model.Category);
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TicketID,Requestor,Category,Status,Title,Content,Solution")] TicketsEditViewModel model)
        {
            User requestor = unitOfWork.UserRepository.GetAll(u => u.Email == model.Requestor).SingleOrDefault();
            User solver = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single();
            Ticket ticket = unitOfWork.TicketRepository.GetById(model.TicketID);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            if (ModelState.IsValid)
            {
                ticket.RequestorID = requestor?.UserID;
                
                ticket.CategoryID = model.Category;

                string previousStatus = ticket.status;
                // If solver doesn't change status and status remains New, but he changes solution, 
                // status changes to Solved
                if (model.Status == "New" && model.Status == previousStatus &&
                    !string.IsNullOrEmpty(model.Solution) && model.Solution != ticket.Solution)
                    ticket.status = "Solved";
                else
                    ticket.status = model.Status;
                if (ticket.status == "New")
                {
                    ticket.SolveOrCloseDate = null;
                    ticket.SolverID = null;
                }
                if ((ticket.status == "Solved" && previousStatus != "Solved") ||
                    (ticket.status == "Closed" && previousStatus != "Closed"))
                {
                    ticket.SolveOrCloseDate = DateTime.Now;
                    ticket.SolverID = solver.UserID;
                }
                ticket.Title = model.Title;
                ticket.Content = model.Content;
                ticket.Solution = model.Solution;
                unitOfWork.TicketRepository.Update(ticket);
                unitOfWork.Save();
                return RedirectToAction("Index");
            }
            model.Creator = ticket.Creator;
            model.Solver = ticket.Solver;
            model.CreateDate = ticket.CreateDate.ToShortDateString() + " " + ticket.CreateDate.ToShortTimeString();
            model.SolveOrCloseDate = ticket.SolveOrCloseDate != null ? ticket.SolveOrCloseDate?.ToShortDateString() + " " + ticket.SolveOrCloseDate?.ToShortTimeString() : null;
            model.UsersList = new SelectList(new[]
            {
                new
                {
                    display = $"{requestor.FirstName} {requestor.LastName} ({requestor.Email})",
                    value = requestor.Email
                }
            }, "value", "display", requestor.Email);

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

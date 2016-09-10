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

        public ActionResult Index([Bind(Include = "Search,SortBy,DescSort,Page")] TicketsIndexViewModel model)
        {
            Expression<Func<Ticket, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                filter = u => u.Title.ToLower().Contains(model.Search.ToLower()) ||
                              u.Content.ToLower().Contains(model.Search.ToLower()) ||
                              u.Solution.ToLower().Contains(model.Search.ToLower());
            }

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
            model.Tickets = unitOfWork.TicketRepository.GetAll(filter: filter, orderBy: orderBy).ToPagedList(model.Page, 2);
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

            List<Category> categories = unitOfWork.CategoryRepository.GetAll(orderBy: c => c.OrderBy(o => o.Name)).ToList();
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

            List<Category> categories = unitOfWork.CategoryRepository.GetAll(orderBy: c => c.OrderBy(o => o.Name)).ToList();
            categories.Insert(0, new Category() { CategoryID = 0, Name = "-" });
            model.Categories = new SelectList(categories, "CategoryID", "Name", model.Category);

            return View(model);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
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

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,FirstName,LastName,Email,Phone,MobilePhone,Company,Department,Admin")] User user)
        {
            if (ModelState.IsValid)
            {
                if (unitOfWork.UserRepository.GetAll(u => u.UserId != user.UserId && u.Email.ToLower() == user.Email.ToLower()).Count() == 0)
                {
                    //unitOfWork.UserRepository.UpdateUserInfo(user);
                    unitOfWork.Save();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", $"Email address {user.Email} exists in database");
                }
            }
            return View(user);
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
                UserID = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName
            });
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPassword([Bind(Include = "UserId,Password,ConfirmPassword")] UsersChangePasswordViewModel user)
        {
            if (ModelState.IsValid)
            {
                User editedUser = new User();
                editedUser.UserId = user.UserID;
                editedUser.Salt = Guid.NewGuid().ToString();
                //editedUser.Password = HashPassword(user.Password, editedUser.Salt);
                //unitOfWork.UserRepository.UpdateUserPassword(editedUser);
                unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            unitOfWork.UserRepository.Delete(id);
            unitOfWork.Save();
            return RedirectToAction("Index");
        }
    }
}

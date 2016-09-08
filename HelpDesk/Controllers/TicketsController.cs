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

        public string FindUsers(string search)
        {
            string result = "";
            foreach (var u in unitOfWork.UserRepository.GetAll(u => (u.FirstName + " " + u.LastName).ToLower().Contains(search.ToLower()) || u.Email.ToLower().Contains(search.ToLower())))
            {
                result += $"<option value={u.UserId}>{u.FirstName} {u.LastName} ({u.Email})</option>";
            }
            return result;
        }

        // GET: Users
        public ActionResult Index(string search = null, string sortBy = "LastName", bool descSort = false, int page = 1)
        {
            ViewBag.search = search;
            ViewBag.sortBy = sortBy;
            ViewBag.descSort = descSort;
            ViewBag.page = page;

            Expression<Func<User, bool>> filter = null;

            if (!string.IsNullOrWhiteSpace(search))
            {
                filter = u => u.FirstName.ToLower().Contains(search.ToLower()) ||
                              u.LastName.ToLower().Contains(search.ToLower()) ||
                              u.Email.ToLower().Contains(search.ToLower()) ||
                              u.Phone.ToLower().Contains(search.ToLower()) ||
                              u.MobilePhone.ToLower().Contains(search.ToLower()) ||
                              u.Company.ToLower().Contains(search.ToLower()) ||
                              u.Department.ToLower().Contains(search.ToLower());                
            }

            Expression<Func<User, object>> propertySelector = null;
            switch (sortBy)
            {
                case "FirstName":
                    propertySelector = u => u.FirstName;
                    break;
                case "LastName":
                    propertySelector = u => u.LastName;
                    break;
                case "Email":
                    propertySelector = u => u.Email;
                    break;
                case "Phone":
                    propertySelector = u => u.Phone;
                    break;
                case "MobilePhone":
                    propertySelector = u => u.MobilePhone;
                    break;
                case "Company":
                    propertySelector = u => u.Company;
                    break;
                case "Department":
                    propertySelector = u => u.Department;
                    break;
                case "Role":
                    propertySelector = u => u.Role;
                    break;
            }
            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null;
            if (propertySelector != null)
            {
                if (descSort)
                    orderBy = x => x.OrderByDescending(propertySelector);
                else
                    orderBy = x => x.OrderBy(propertySelector);
            }
           
            return View(unitOfWork.UserRepository.GetAll(filter: filter, orderBy: orderBy).ToPagedList(page, 2));
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

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.user = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single();
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RequestorID")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                //if (unitOfWork.UserRepository.GetAll(u => u.Email.ToLower() == ticket.Email.ToLower()).Count() == 0)
                //{
                //    User newUser = new User
                //    {
                //        FirstName = ticket.FirstName,
                //        LastName = ticket.LastName,
                //        Email = ticket.Email,
                //        Phone = ticket.Phone,
                //        MobilePhone = ticket.MobilePhone,
                //        Company = ticket.Company,
                //        Department = ticket.Department,
                //        Role = ticket.Role
                //    };

                //    newUser.Salt = Guid.NewGuid().ToString();
                //    newUser.Password = HashPassword(ticket.Password, newUser.Salt);
                return View(ticket);


                ticket.CreateDate = DateTime.Now;
                ticket.CategoryID = 1;
                ticket.SolveOrCloseDate = DateTime.Now;
                ticket.SolverID = 1;
                
                    unitOfWork.TicketRepository.Insert(ticket);
                    unitOfWork.Save();
                    return RedirectToAction("Index", "Home");
                //}
                //else
                //{
                //    ModelState.AddModelError("", $"Email address {ticket.Email} exists in database");
                //}
            }
            return View(ticket);
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

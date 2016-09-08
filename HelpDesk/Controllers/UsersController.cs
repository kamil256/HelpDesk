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
using static HelpDesk.Infrastructure.Utilities;
using System.Linq.Expressions;
using PagedList;

namespace HelpDesk.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private IUnitOfWork unitOfWork;

        public UsersController()
        {
            unitOfWork = new UnitOfWork();
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

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FirstName,LastName,Email,Password,ConfirmPassword,Phone,MobilePhone,Company,Department,Role")] User user)
        {
            ModelState.Remove("HashedPassword");
            ModelState.Remove("Salt");
            if (ModelState.IsValid)
            {
                if (unitOfWork.UserRepository.GetAll(u => u.Email.ToLower() == user.Email.ToLower()).Count() > 0)
                    ModelState.AddModelError("Email", $"The email address is in use");
                else
                {
                    user.Salt = Guid.NewGuid().ToString();
                    user.HashedPassword = HashPassword(user.Password, user.Salt);
                    unitOfWork.UserRepository.Insert(user);
                    unitOfWork.Save();
                    return RedirectToAction("Index");
                }
            }
            return View(user);
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
                    unitOfWork.UserRepository.UpdateUserInfo(user);
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

            return View(new EditPasswordViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName
            });
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPassword([Bind(Include = "UserId,Password,ConfirmPassword")] EditPasswordViewModel user)
        {
            if (ModelState.IsValid)
            {
                User editedUser = new User();
                editedUser.UserId = user.UserId;
                editedUser.Salt = Guid.NewGuid().ToString();
                editedUser.Password = HashPassword(user.Password, editedUser.Salt);
                unitOfWork.UserRepository.UpdateUserPassword(editedUser);
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

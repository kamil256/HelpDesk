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
    public class UsersController : Controller
    {
        private IUnitOfWork unitOfWork;

        public UsersController()
        {
            unitOfWork = new UnitOfWork();
        }

        public ActionResult Index([Bind(Include = "Search,SortBy,DescSort,Page")] UsersIndexViewModel model)
        {
            Expression<Func<User, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                filter = u => u.FirstName.ToLower().Contains(model.Search.ToLower()) ||
                              u.LastName.ToLower().Contains(model.Search.ToLower()) ||
                              u.Email.ToLower().Contains(model.Search.ToLower()) ||
                              u.Phone.ToLower().Contains(model.Search.ToLower()) ||
                              u.MobilePhone.ToLower().Contains(model.Search.ToLower()) ||
                              u.Company.ToLower().Contains(model.Search.ToLower()) ||
                              u.Department.ToLower().Contains(model.Search.ToLower());                
            }

            Expression<Func<User, object>> propertySelector = null;
            switch (model.SortBy)
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
                case "Tickets":
                    propertySelector = u => u.RequestedTickets.Count.ToString();
                    break;
            }
            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null;
            if (propertySelector != null)
            {
                if (model.DescSort)
                    orderBy = x => x.OrderByDescending(propertySelector);
                else
                    orderBy = x => x.OrderBy(propertySelector);
            }
            model.Users = unitOfWork.UserRepository.GetAll(filter: filter, orderBy: orderBy).ToPagedList(model.Page, 2);
            return View(model);
        }

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
        public ActionResult Create([Bind(Include = "FirstName,LastName,Email,Password,ConfirmPassword,Phone,MobilePhone,Company,Department,Role")] UsersCreateViewModel user)
        {
            if (ModelState.IsValid)
            {
                if (unitOfWork.UserRepository.GetAll(u => u.Email.ToLower() == user.Email.ToLower()).Count() > 0)
                    ModelState.AddModelError("Email", $"The email address is in use");
                else
                {
                    User newUser = new User
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Phone = user.Phone,
                        MobilePhone = user.MobilePhone,
                        Company = user.Company,
                        Department = user.Department,
                        Role = user.Role
                    };
                    newUser.Salt = Guid.NewGuid().ToString();
                    newUser.HashedPassword = HashPassword(user.Password, newUser.Salt);
                    unitOfWork.UserRepository.Insert(newUser);
                    unitOfWork.Save();
                    return RedirectToAction("Index");
                }
            }
            return View(user);
        }

        public ActionResult Edit(int id = 0)
        {
            User user = unitOfWork.UserRepository.GetById(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(new UsersEditViewModel
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                MobilePhone = user.MobilePhone,
                Company = user.Company,
                Department = user.Department,
                Role = user.Role,
                Tickets = user.RequestedTickets.OrderByDescending(t => t.CreateDate)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,FirstName,LastName,Email,Phone,MobilePhone,Company,Department,Role")] UsersEditViewModel user)
        {
            if (ModelState.IsValid)
            {
                if (unitOfWork.UserRepository.GetAll(u => u.UserID != user.UserID && u.Email.ToLower() == user.Email.ToLower()).Count() > 0)
                    ModelState.AddModelError("Email", $"The email address is in use");
                else
                {
                    User editedUser = unitOfWork.UserRepository.GetById(user.UserID);
                    editedUser.FirstName = user.FirstName;
                    editedUser.LastName = user.LastName;
                    editedUser.Email = user.Email;
                    editedUser.Phone = user.Phone;
                    editedUser.MobilePhone = user.MobilePhone;
                    editedUser.Company = user.Company;
                    editedUser.Department = user.Department;
                    editedUser.Role = user.Role;
                    unitOfWork.UserRepository.Update(editedUser);
                    unitOfWork.Save();
                    return RedirectToAction("Index");
                }
            }
            user.Tickets = unitOfWork.UserRepository.GetById(user.UserID).RequestedTickets.OrderByDescending(t => t.CreateDate);
            return View(user);
        }

        public ActionResult ChangePassword(int id = 0)
        {
            User user = unitOfWork.UserRepository.GetById(id);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword([Bind(Include = "UserID,Password,ConfirmPassword")] UsersChangePasswordViewModel user)
        {
            if (ModelState.IsValid)
            {
                User editedUser = unitOfWork.UserRepository.GetById(user.UserID);
                editedUser.Salt = Guid.NewGuid().ToString();
                editedUser.HashedPassword = HashPassword(user.Password, editedUser.Salt);
                unitOfWork.UserRepository.Update(editedUser);
                unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        public ActionResult Delete(int id = 0)
        {
            User user = unitOfWork.UserRepository.GetById(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            foreach (Ticket ticket in unitOfWork.UserRepository.GetById(id).CreatedTickets)
                ticket.Solver = null;
            foreach (Ticket ticket in unitOfWork.UserRepository.GetById(id).RequestedTickets)
                ticket.Solver = null;
            foreach (Ticket ticket in unitOfWork.UserRepository.GetById(id).SolvedTickets)
                ticket.Solver = null;
            unitOfWork.UserRepository.Delete(id);
            unitOfWork.Save();
            return RedirectToAction("Index");
        }
    }
}

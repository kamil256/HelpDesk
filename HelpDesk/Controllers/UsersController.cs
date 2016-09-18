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

            Expression<Func<User, object>> orderByPropertySelector = null;
            switch (model.SortBy)
            {
                case "FirstName":
                    orderByPropertySelector = u => u.FirstName;
                    break;
                case "LastName":
                    orderByPropertySelector = u => u.LastName;
                    break;
                case "Email":
                    orderByPropertySelector = u => u.Email;
                    break;
                case "Phone":
                    orderByPropertySelector = u => u.Phone;
                    break;
                case "MobilePhone":
                    orderByPropertySelector = u => u.MobilePhone;
                    break;
                case "Company":
                    orderByPropertySelector = u => u.Company;
                    break;
                case "Department":
                    orderByPropertySelector = u => u.Department;
                    break;
                case "Role":
                    orderByPropertySelector = u => u.Role;
                    break;
                case "Tickets":
                    orderByPropertySelector = u => u.RequestedTickets.Count.ToString();
                    break;
            }
            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null;
            if (orderByPropertySelector != null)
            {
                if (model.DescSort)
                    orderBy = x => x.OrderByDescending(orderByPropertySelector);
                else
                    orderBy = x => x.OrderBy(orderByPropertySelector);
            }
            model.Users = unitOfWork.UserRepository.GetAll(filter: filter, orderBy: orderBy).ToPagedList(model.Page, 2);
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FirstName,LastName,Email,Password,ConfirmPassword,Phone,MobilePhone,Company,Department,Role")] UsersCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (unitOfWork.UserRepository.GetAll(u => u.Email.ToLower() == model.Email.ToLower()).Count() > 0)
                        ModelState.AddModelError("Email", $"The email address is in use");
                    else
                    {
                        User user = new User
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Email = model.Email,
                            Phone = model.Phone,
                            MobilePhone = model.MobilePhone,
                            Company = model.Company,
                            Department = model.Department,
                            Role = model.Role
                        };
                        user.Salt = Guid.NewGuid().ToString();
                        user.HashedPassword = HashPassword(model.Password, user.Salt);
                        unitOfWork.UserRepository.Insert(user);
                        unitOfWork.Save();
                        TempData["Success"] = "Successfully added new user!";
                        return RedirectToAction("Index");
                    }
                }
            }
            catch
            {
                ModelState.AddModelError("", "Cannot create user. Try again!");
            }
            return View(model);
        }

        public ActionResult Edit(int id = 0)
        {
            //User user = unitOfWork.UserRepository.GetById(id);
            User user = unitOfWork.UserRepository.GetAll(filter: u => u.UserID == id, includeProperties: "CreatedTickets").SingleOrDefault();
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
                Tickets = user.CreatedTickets.OrderByDescending(t => t.CreatedOn)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,FirstName,LastName,Email,Password,ConfirmPassword,Phone,MobilePhone,Company,Department,Role")] UsersEditViewModel model)
        {
            User user = unitOfWork.UserRepository.GetAll(filter: u => u.UserID == model.UserID, includeProperties: "CreatedTickets").SingleOrDefault();
            if (user == null)
            {
                return HttpNotFound();
            }
            try
            { 
                if (ModelState.IsValid)
                {
                    if (unitOfWork.UserRepository.GetAll(u => u.UserID != model.UserID && u.Email.ToLower() == model.Email.ToLower()).Count() > 0)
                        ModelState.AddModelError("Email", $"The email address is in use");
                    else
                    {
                        user.FirstName = model.FirstName;
                        user.LastName = model.LastName;
                        user.Email = model.Email;
                        if (model.Password != null)
                        {
                            user.Salt = Guid.NewGuid().ToString();
                            user.HashedPassword = HashPassword(model.Password, user.Salt);
                        }
                        user.Phone = model.Phone;
                        user.MobilePhone = model.MobilePhone;
                        user.Company = model.Company;
                        user.Department = model.Department;
                        user.Role = model.Role;
                        unitOfWork.UserRepository.Update(user);
                        unitOfWork.Save();
                        TempData["Success"] = "Successfully edited user!";
                        return RedirectToAction("Index");
                    }
                }
            }
            catch
            {
                ModelState.AddModelError("", "Cannot edit user. Try again!");
            }
            model.Tickets = user.CreatedTickets.OrderByDescending(t => t.CreatedOn);

            return View(model);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                // Must be eager loading to load tickets and set null to their User type properties
                User user = unitOfWork.UserRepository.GetAll(filter: u => u.UserID == id, includeProperties: "CreatedTickets,RequestedTickets,AssignedTickets").SingleOrDefault();
                unitOfWork.UserRepository.Delete(user);
                unitOfWork.Save();
                TempData["Success"] = "Successfully deleted user!";
            }
            catch
            {
                TempData["Fail"] = "Cannot delete user. Try again!";
            }
            return RedirectToAction("Index");
        }
    }
}

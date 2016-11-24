using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using HelpDesk.UI.ViewModels.Users;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace HelpDesk.UI.Controllers.WebAPI
{
    public class UsersController : ApiController
    {
        private UserManager UserManager
        {
            get
            {
                return HttpContext.Current.Request.GetOwinContext().GetUserManager<UserManager>();
            }
        }

        private RoleManager RoleManager
        {
            get
            {
                return HttpContext.Current.Request.GetOwinContext().GetUserManager<RoleManager>();
            }
        }

        private User CurrentUser
        {
            get
            {
                return UserManager.FindByNameAsync(User.Identity.Name).Result;
            }
        }

        private IUnitOfWork unitOfWork;

        public UsersController()//IUnitOfWork unitOfWork)
        {
            this.unitOfWork = new UnitOfWork();
        }

        [OverrideAuthorization]
        [HttpGet]
        public PagedUsers GetUsers([FromUri] UserFilteringModel model)
        {
            List<Expression<Func<User, bool>>> filters = new List<Expression<Func<User, bool>>>();
            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                filters.Add(u => u.FirstName.ToLower().Contains(model.Search.ToLower()) ||
                                 u.LastName.ToLower().Contains(model.Search.ToLower()) ||
                                 u.Email.ToLower().Contains(model.Search.ToLower()) ||
                                 u.Phone.ToLower().Contains(model.Search.ToLower()) ||
                                 u.MobilePhone.ToLower().Contains(model.Search.ToLower()) ||
                                 u.Company.ToLower().Contains(model.Search.ToLower()) ||
                                 u.Department.ToLower().Contains(model.Search.ToLower()));
            }

            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null;

            switch (model.SortBy)
            {
                case "First name":
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(u => u.FirstName);
                    else
                        orderBy = query => query.OrderBy(u => u.FirstName);
                    break;
                case "Email":
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(u => u.Email);
                    else
                        orderBy = query => query.OrderBy(u => u.Email);
                    break;
                case "Phone":
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(u => u.Phone);
                    else
                        orderBy = query => query.OrderBy(u => u.Phone);
                    break;
                case "Mobile phone":
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(u => u.MobilePhone);
                    else
                        orderBy = query => query.OrderBy(u => u.MobilePhone);
                    break;
                case "Company":
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(u => u.Company);
                    else
                        orderBy = query => query.OrderBy(u => u.Company);
                    break;
                case "Department":
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(u => u.Department);
                    else
                        orderBy = query => query.OrderBy(u => u.Department);
                    break;
                case "Role":
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(u => u.Roles.FirstOrDefault().RoleId);
                    else
                        orderBy = query => query.OrderBy(u => u.Roles.FirstOrDefault().RoleId);
                    break;
                case "Tickets":
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(u => u.CreatedTickets.Count);
                    else
                        orderBy = query => query.OrderBy(u => u.CreatedTickets.Count);
                    break;
                case "Last name":
                default:
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(u => u.LastName);
                    else
                        orderBy = query => query.OrderBy(u => u.LastName);
                    break;
            }            

            if (!string.IsNullOrEmpty(model.Role))
            {
                string roleId = RoleManager.FindByName(model.Role).Id;//unitOfWork.RoleRepository.Get().FirstOrDefault(r => r.Name == model.Role).Id;
                filters.Add(u => u.Roles.FirstOrDefault(r => r.RoleId == roleId) != null);
            }

            PagedUsers pagedUsers = new PagedUsers();
            string adminRoleId = RoleManager.FindByName("Admin").Id;//unitOfWork.RoleRepository.Get().FirstOrDefault(r => r.Name == "Admin").Id;

            int skip = 0;
            int take = 0;

            if (!model.IgnorePaging)
            {
                int usersPerPage = CurrentUser.Settings.UsersPerPage;
                int numberOfUsers = UserManager.Users.Count();//unitOfWork.UserRepository.Get(filters: filters, orderBy: orderBy).Count();
                int numberOfPages = (int)Math.Ceiling((decimal)numberOfUsers / usersPerPage);

                skip = (model.Page - 1) * usersPerPage;
                take = usersPerPage;

                pagedUsers.NumberOfPages = numberOfPages;
            }

            IQueryable<User> users = UserManager.Users;
            if (filters != null)
                foreach (var filter in filters)
                    if (filter != null)
                        users = users.Where(filter);
            if (orderBy != null)
                users = orderBy(users);
            if (!model.IgnorePaging)
            {
                users = users.Skip(skip).Take(take);
            }
            pagedUsers.Users = users.Select(u => new UserDTO//unitOfWork.UserRepository.Get(filters: filters, orderBy: orderBy, skip: skip, take: take).Select(u => new UserDTO
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                MobilePhone = u.MobilePhone,
                Company = u.Company,
                Department = u.Department,
                Role = u.Roles.FirstOrDefault(r => r.RoleId == adminRoleId) != null ? "Admin" : "User",
                Tickets = u.CreatedTickets.Count
            });         

            return pagedUsers;
        }
    }
}

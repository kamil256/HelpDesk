using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using HelpDesk.UI.Infrastructure;
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
        private readonly IUnitOfWork unitOfWork;
        private readonly IdentityHelper identityHelper;

        public UsersController()
        {
            this.unitOfWork = new UnitOfWork();
            this.identityHelper = new IdentityHelper();
        }

        [OverrideAuthorization]
        [HttpGet]
        public HttpResponseMessage GetUsers(string role = null, string search = null, bool advancedSearch = true, string sortBy = null, bool descSort = false, int page = 0, int? usersPerPage = null)
        {
            List<Expression<Func<User, bool>>> filters = new List<Expression<Func<User, bool>>>();

            if (!string.IsNullOrEmpty(role))
            {
                string roleId = identityHelper.RoleManager.FindByName(role).Id;
                filters.Add(u => u.Roles.FirstOrDefault(r => r.RoleId == roleId) != null);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                if (!advancedSearch)
                    filters.Add(u => u.FirstName.ToLower().Contains(search.ToLower()) ||
                                     u.LastName.ToLower().Contains(search.ToLower()) ||
                                     u.Email.ToLower().Contains(search.ToLower()));
                else
                    filters.Add(u => u.FirstName.ToLower().Contains(search.ToLower()) ||
                                     u.LastName.ToLower().Contains(search.ToLower()) ||
                                     u.Email.ToLower().Contains(search.ToLower()) ||
                                     u.Phone.ToLower().Contains(search.ToLower()) ||
                                     u.MobilePhone.ToLower().Contains(search.ToLower()) ||
                                     u.Company.ToLower().Contains(search.ToLower()) ||
                                     u.Department.ToLower().Contains(search.ToLower()));
            }

            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null;

            switch (sortBy)
            {
                case "First name":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.FirstName);
                    else
                        orderBy = query => query.OrderBy(u => u.FirstName);
                    break;
                case "Email":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.Email);
                    else
                        orderBy = query => query.OrderBy(u => u.Email);
                    break;
                case "Phone":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.Phone);
                    else
                        orderBy = query => query.OrderBy(u => u.Phone);
                    break;
                case "Mobile phone":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.MobilePhone);
                    else
                        orderBy = query => query.OrderBy(u => u.MobilePhone);
                    break;
                case "Company":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.Company);
                    else
                        orderBy = query => query.OrderBy(u => u.Company);
                    break;
                case "Department":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.Department);
                    else
                        orderBy = query => query.OrderBy(u => u.Department);
                    break;
                case "Role":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.Roles.FirstOrDefault().RoleId);
                    else
                        orderBy = query => query.OrderBy(u => u.Roles.FirstOrDefault().RoleId);
                    break;
                case "Tickets":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.CreatedTickets.Count);
                    else
                        orderBy = query => query.OrderBy(u => u.CreatedTickets.Count);
                    break;
                case "Last name":
                default:
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.LastName);
                    else
                        orderBy = query => query.OrderBy(u => u.LastName);
                    break;
            }

            usersPerPage = usersPerPage ?? identityHelper.CurrentUser.Settings.UsersPerPage;
            int numberOfUsers = identityHelper.UserManager.Users.Count();
            int numberOfPages;

            IQueryable<User> users = identityHelper.UserManager.Users;

            if (filters != null)
                foreach (var filter in filters)
                    if (filter != null)
                        users = users.Where(filter);

            if (orderBy != null)
                users = orderBy(users);

            if (page != 0)
            {
                numberOfPages = (int)Math.Ceiling((decimal)numberOfUsers / (int)usersPerPage);
                users = users.Skip((page - 1) * (int)usersPerPage).Take((int)usersPerPage);
            }
            else
            {
                numberOfPages = 1;
            }

            string adminRoleId = identityHelper.RoleManager.FindByName("Admin").Id;

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Users = users.Select(u => new UserDTO
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
                    TicketsCount = u.CreatedTickets.Count
                }),
                NumberOfPages = numberOfPages,
                NumberOfUsers = numberOfUsers
            });
        }
    }
}

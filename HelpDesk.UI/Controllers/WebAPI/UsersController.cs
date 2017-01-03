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
    [Authorize(Roles = "Admin")]
    public class UsersController : ApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IdentityHelper identityHelper;

        public UsersController()
        {
            this.unitOfWork = new UnitOfWork();
            this.identityHelper = new IdentityHelper();
        }

        [HttpGet]
        [OverrideAuthorization]
        [Authorize]
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
                    IQueryable<Role> allRoles = identityHelper.RoleManager.Roles;
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.Roles.Join(allRoles, ur => ur.RoleId, r => r.Id, (ur, r) => new { r.Name }).FirstOrDefault().Name);
                    else
                        orderBy = query => query.OrderBy(u => u.Roles.Join(allRoles, ur => ur.RoleId, r => r.Id, (ur, r) => new { r.Name }).FirstOrDefault().Name);
                    break;
                case "Last activity":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.LastActivity);
                    else
                        orderBy = query => query.OrderBy(u => u.LastActivity);
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
            int numberOfUsersFound = numberOfUsers;
            int numberOfPages;

            IQueryable<User> users = identityHelper.UserManager.Users;

            if (filters != null)
                foreach (var filter in filters)
                    if (filter != null)
                    {
                        users = users.Where(filter);
                        numberOfUsersFound = users.Count();
                    }

            if (orderBy != null)
                users = orderBy(users);

            if (page != 0)
            {
                numberOfPages = (int)Math.Ceiling((decimal)numberOfUsersFound / (int)usersPerPage);
                users = users.Skip((page - 1) * (int)usersPerPage).Take((int)usersPerPage);
            }
            else
            {
                numberOfPages = 1;
            }

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Users = users.AsEnumerable().Select(u => new UserDTO
                {
                    UserId = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Phone = u.Phone,
                    MobilePhone = u.MobilePhone,
                    Company = u.Company,
                    Department = u.Department,
                    Role = identityHelper.UserManager.IsInRole(u.Id, "Admin") ? "Admin" : "User",
                    LastActivity = u.LastActivity != null ? ((DateTime)u.LastActivity).ToString("yyyy-MM-dd HH:mm") : "Never",
                    TicketsCount = u.CreatedTickets.Union(u.RequestedTickets).Distinct().Count()
                }),
                NumberOfPages = numberOfPages,
                NumberOfUsers = numberOfUsers,
                NumberOfUsersFound = numberOfUsersFound
            });
        }
    }
}

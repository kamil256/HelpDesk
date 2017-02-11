using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using HelpDesk.UI.Infrastructure;
using HelpDesk.UI.Infrastructure.Abstract;
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
        private readonly IIdentityHelper identityHelper;
        private readonly IUnitOfWork unitOfWork;

        public UsersController(IUnitOfWork unitOfWork, IIdentityHelper identityHelper)
        {
            this.unitOfWork = unitOfWork;
            this.identityHelper = identityHelper;
        }

        [HttpGet]
        [OverrideAuthorization]
        [Authorize]
        public IHttpActionResult GetUsers(bool? active = null, string role = null, string search = null, bool searchAllWords = false, string sortBy = "Last name", bool descSort = false, int page = 0, int? usersPerPage = null)
        {
            List<Expression<Func<User, bool>>> filters = new List<Expression<Func<User, bool>>>();

            if (active != null)
                filters.Add(u => u.Active == active);

            if (!string.IsNullOrEmpty(role))
            {
                string roleId = identityHelper.GetRoleId(role);
                filters.Add(u => u.Roles.FirstOrDefault(r => r.RoleId == roleId) != null);
            }

            search = search.RemoveExcessSpaces();
            if (!string.IsNullOrEmpty(search))
            {
                string[] words = search.Split(' ');
                if (searchAllWords)
                    filters.Add(u => words.All(w => u.FirstName.ToLower().Contains(w.ToLower()) ||
                                                    u.LastName.ToLower().Contains(w.ToLower()) ||
                                                    u.Email.ToLower().Contains(w.ToLower()) ||
                                                    u.Phone.ToLower().Contains(w.ToLower()) ||
                                                    u.MobilePhone.ToLower().Contains(w.ToLower()) ||
                                                    u.Company.ToLower().Contains(w.ToLower()) ||
                                                    u.Department.ToLower().Contains(w.ToLower())));
                else
                    filters.Add(u => words.Any(w => u.FirstName.ToLower().Contains(w.ToLower()) ||
                                                    u.LastName.ToLower().Contains(w.ToLower()) ||
                                                    u.Email.ToLower().Contains(w.ToLower()) ||
                                                    u.Phone.ToLower().Contains(w.ToLower()) ||
                                                    u.MobilePhone.ToLower().Contains(w.ToLower()) ||
                                                    u.Company.ToLower().Contains(w.ToLower()) ||
                                                    u.Department.ToLower().Contains(w.ToLower())));
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
                    //IQueryable<Role> allRoles = identityHelper.RoleManager.Roles;
                    //if (descSort)
                    //    orderBy = query => query.OrderByDescending(u => u.Roles.Join(allRoles, ur => ur.RoleId, r => r.Id, (ur, r) => new { r.Name }).FirstOrDefault().Name);
                    //else
                    //    orderBy = query => query.OrderBy(u => u.Roles.Join(allRoles, ur => ur.RoleId, r => r.Id, (ur, r) => new { r.Name }).FirstOrDefault().Name);
                    break;
                case "Last activity":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.LastActivity);
                    else
                        orderBy = query => query.OrderBy(u => u.LastActivity);
                    break;
                case "Tickets":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.CreatedTickets.Union(u.RequestedTickets).Distinct().Count());
                    else
                        orderBy = query => query.OrderBy(u => u.CreatedTickets.Union(u.RequestedTickets).Distinct().Count());
                    break;
                case "Last name":
                default:
                    if (descSort)
                        orderBy = query => query.OrderByDescending(u => u.LastName);
                    else
                        orderBy = query => query.OrderBy(u => u.LastName);
                    break;
            }

            usersPerPage = usersPerPage ?? identityHelper.UsersPerPageSettingOfCurrentUser;
            int numberOfUsers = unitOfWork.UserRepository.Get(filters).Count();
            int numberOfPages;

            IEnumerable<User> users;
            if (page != 0)
            {
                numberOfPages = (int)Math.Ceiling((decimal)numberOfUsers / (int)usersPerPage);
                users = unitOfWork.UserRepository.Get(filters: filters, orderBy: orderBy, skip: (page - 1) * (int)usersPerPage, take: (int)usersPerPage);
            }
            else
            {
                numberOfPages = 1;
                users = unitOfWork.UserRepository.Get(filters: filters, orderBy: orderBy);
            }

            return Ok(new UserResponse
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
                    Role = identityHelper.IsUserAnAdministrator(u.Id) ? "Admin" : "User",
                    Active = u.Active,
                    LastActivity = u.LastActivity != null ? ((DateTime)u.LastActivity).ToString("yyyy-MM-dd HH:mm") : "Never",
                    TicketsCount = u.CreatedTickets.Union(u.RequestedTickets).Distinct().Count()
                }),
                NumberOfPages = numberOfPages,
                FoundItemsCount = numberOfUsers,
                TotalItemsCount = identityHelper.TotalUsersCount
            });
        }
    }
}

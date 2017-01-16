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
        private readonly IdentityHelper identityHelper;

        public UsersController()
        {
            this.identityHelper = new IdentityHelper();
        }

        private string removeExcessSpaces(string text)
        {
            if (text != null)
            {
                System.Text.RegularExpressions.Regex trimmer = new System.Text.RegularExpressions.Regex(@"\s\s+");
                return trimmer.Replace(text.Trim(), " ");
            }
            else
                return text;
        }

        [HttpGet]
        [OverrideAuthorization]
        [Authorize]
        public HttpResponseMessage GetUsers(bool? active = null, string role = null, string search = null, bool searchAllWords = false, string sortBy = "Last name", bool descSort = false, int page = 0, int? usersPerPage = null)
        {
            IQueryable<User> users = identityHelper.UserManager.Users;

            if (active != null)
                users = users.Where(u => u.Active == active);

            if (!string.IsNullOrEmpty(role))
            {
                string roleId = identityHelper.RoleManager.FindByName(role).Id;
                users = users.Where(u => u.Roles.FirstOrDefault(r => r.RoleId == roleId) != null);
            }

            search = removeExcessSpaces(search);
            if (!string.IsNullOrEmpty(search))
            {
                string[] words = search.Split(' ');
                if (searchAllWords)
                    users = users.Where(u => words.All(w => u.FirstName.ToLower().Contains(w.ToLower()) ||
                                                            u.LastName.ToLower().Contains(w.ToLower()) ||
                                                            u.Email.ToLower().Contains(w.ToLower()) ||
                                                            u.Phone.ToLower().Contains(w.ToLower()) ||
                                                            u.MobilePhone.ToLower().Contains(w.ToLower()) ||
                                                            u.Company.ToLower().Contains(w.ToLower()) ||
                                                            u.Department.ToLower().Contains(w.ToLower())));
                else
                    users = users.Where(u => words.Any(w => u.FirstName.ToLower().Contains(w.ToLower()) ||
                                                            u.LastName.ToLower().Contains(w.ToLower()) ||
                                                            u.Email.ToLower().Contains(w.ToLower()) ||
                                                            u.Phone.ToLower().Contains(w.ToLower()) ||
                                                            u.MobilePhone.ToLower().Contains(w.ToLower()) ||
                                                            u.Company.ToLower().Contains(w.ToLower()) ||
                                                            u.Department.ToLower().Contains(w.ToLower())));
            }

            switch (sortBy)
            {
                case "First name":
                    if (descSort)
                        users = users.OrderByDescending(u => u.FirstName);
                    else
                        users = users.OrderBy(u => u.FirstName);
                    break;
                case "Email":
                    if (descSort)
                        users = users.OrderByDescending(u => u.Email);
                    else
                        users = users.OrderBy(u => u.Email);
                    break;
                case "Phone":
                    if (descSort)
                        users = users.OrderByDescending(u => u.Phone);
                    else
                        users = users.OrderBy(u => u.Phone);
                    break;
                case "Mobile phone":
                    if (descSort)
                        users = users.OrderByDescending(u => u.MobilePhone);
                    else
                        users = users.OrderBy(u => u.MobilePhone);
                    break;
                case "Company":
                    if (descSort)
                        users = users.OrderByDescending(u => u.Company);
                    else
                        users = users.OrderBy(u => u.Company);
                    break;
                case "Department":
                    if (descSort)
                        users = users.OrderByDescending(u => u.Department);
                    else
                        users = users.OrderBy(u => u.Department);
                    break;
                case "Role":
                    IQueryable<Role> allRoles = identityHelper.RoleManager.Roles;
                    if (descSort)
                        users = users.OrderByDescending(u => u.Roles.Join(allRoles, ur => ur.RoleId, r => r.Id, (ur, r) => new { r.Name }).FirstOrDefault().Name);
                    else
                        users = users.OrderBy(u => u.Roles.Join(allRoles, ur => ur.RoleId, r => r.Id, (ur, r) => new { r.Name }).FirstOrDefault().Name);
                    break;
                case "Last activity":
                    if (descSort)
                        users = users.OrderByDescending(u => u.LastActivity);
                    else
                        users = users.OrderBy(u => u.LastActivity);
                    break;
                case "Tickets":
                    if (descSort)
                        users = users.OrderByDescending(u => u.CreatedTickets.Union(u.RequestedTickets).Distinct().Count());
                    else
                        users = users.OrderBy(u => u.CreatedTickets.Union(u.RequestedTickets).Distinct().Count());
                    break;
                case "Last name":
                default:
                    if (descSort)
                        users = users.OrderByDescending(u => u.LastName);
                    else
                        users = users.OrderBy(u => u.LastName);
                    break;
            }

            usersPerPage = usersPerPage ?? identityHelper.CurrentUser.Settings.UsersPerPage;
            int numberOfUsers = users.Count();

            int numberOfPages;
            if (page != 0)
            {
                numberOfPages = (int)Math.Ceiling((decimal)numberOfUsers / (int)usersPerPage);
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
                    Active = u.Active,
                    LastActivity = u.LastActivity != null ? ((DateTime)u.LastActivity).ToString("yyyy-MM-dd HH:mm") : "Never",
                    TicketsCount = u.CreatedTickets.Union(u.RequestedTickets).Distinct().Count()
                }),
                NumberOfPages = numberOfPages,
                FoundItemsCount = numberOfUsers,
                TotalItemsCount = identityHelper.UserManager.Users.Count()
            });
        }
    }
}

using HelpDesk.BLL.Abstract;
using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using HelpDesk.BBL.ExtensionMethods;

namespace HelpDesk.BLL.Concrete
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public PagedUsersList GetPagedUsersList(string loggedInUserId, bool? active = null, string role = null, string search = null, bool searchAllWords = false, string sortBy = "Last name", bool descSort = false, int? page = null, int? usersPerPage = null)
        {
            List<Expression<Func<User, bool>>> filters = new List<Expression<Func<User, bool>>>();

            if (active != null)
                filters.Add(u => u.Active == active);

            if (!string.IsNullOrEmpty(role))
            {
                string roleId = unitOfWork.RoleRepository.Get(new List<Expression<Func<Role, bool>>> { r => r.Name == role }).FirstOrDefault().Id;
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
                    IQueryable<Role> allRoles = unitOfWork.RoleRepository.Get().AsQueryable<Role>();
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

            usersPerPage = usersPerPage ?? unitOfWork.UserRepository.GetById(loggedInUserId).Settings.UsersPerPage;

            int numberOfPages;
            int foundUsersCount = unitOfWork.UserRepository.Count(filters.ToArray());
            int totalUsersCount = unitOfWork.UserRepository.Count();

            IEnumerable<User> users;
            if (page != null)
            {
                numberOfPages = (int)Math.Ceiling((decimal)foundUsersCount / (int)usersPerPage);
                users = unitOfWork.UserRepository.Get(filters: filters, orderBy: orderBy, skip: ((int)page - 1) * (int)usersPerPage, take: (int)usersPerPage);
            }
            else
            {
                numberOfPages = 1;
                users = unitOfWork.UserRepository.Get(filters: filters, orderBy: orderBy);
            }

            return new PagedUsersList
            {
                Users = users,
                NumberOfPages = numberOfPages,
                FoundItemsCount = foundUsersCount,
                TotalItemsCount = totalUsersCount
            };
        }
    }
}

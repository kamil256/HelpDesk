using HelpDesk.DAL;
using HelpDesk.Entities;
using HelpDesk.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace HelpDesk.Controllers
{
    public class ApiUsersController : ApiController
    {
        private IUnitOfWork unitOfWork;

        public ApiUsersController()
        {
            unitOfWork = new UnitOfWork();
        }

        [OverrideAuthorization]
        [HttpGet]
        public PagedUsers GetUsers([FromUri] UserFilteringModel model)
        {
            List<Expression<Func<AppUser, bool>>> filters = new List<Expression<Func<AppUser, bool>>>();
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

            Func<IQueryable<AppUser>, IOrderedQueryable<AppUser>> orderBy = null;

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
                string roleId = unitOfWork.RoleRepository.Get().FirstOrDefault(r => r.Name == model.Role).Id;
                filters.Add(u => u.Roles.FirstOrDefault(r => r.RoleId == roleId) != null);
            }

            PagedUsers pagedUsers = new PagedUsers();
            string adminRoleId = unitOfWork.RoleRepository.Get().FirstOrDefault(r => r.Name == "Admin").Id;
            
            int skip = 0;
            int take = 0;

            if (!model.IgnorePaging)
            {
                int usersPerPage = 1;
                int numberOfUsers = unitOfWork.UserRepository.Get(filters: filters, orderBy: orderBy).Count();
                int numberOfPages = (int)Math.Ceiling((decimal)numberOfUsers / usersPerPage);

                skip = (model.Page - 1) * usersPerPage;
                take = usersPerPage;

                pagedUsers.NumberOfPages = numberOfPages;
            }

            pagedUsers.Users = unitOfWork.UserRepository.Get(filters: filters, orderBy: orderBy, skip: skip, take: take).Select(u => new UserDTO
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                MobilePhone = u.MobilePhone,
                Company = u.Company,
                Department = u.Department,
                Role = u.Roles.FirstOrDefault(r => r.RoleId == adminRoleId) != null ? "Admin" : "User"
            });            

            return pagedUsers;
        }
    }
}

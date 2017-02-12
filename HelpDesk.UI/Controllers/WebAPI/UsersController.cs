using HelpDesk.BLL;
using HelpDesk.BLL.Abstract;
using HelpDesk.BLL.Concrete;
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
        private readonly IUserService userService;
        private readonly IRoleService roleService;
        private readonly IIdentityHelper identityHelper;

        public UsersController(IUserService userService, IRoleService roleService, IIdentityHelper identityHelper)
        {
            this.userService = userService;
            this.roleService = roleService;
            this.identityHelper = identityHelper;
        }

        [HttpGet]
        [OverrideAuthorization]
        [Authorize]
        public IHttpActionResult GetUsers(bool? active = null, string role = null, string search = null, bool searchAllWords = false, string sortBy = "Last name", bool descSort = false, int page = 0, int? usersPerPage = null)
        {
            PagedUsersList pagedUsersList = userService.GetPagedUsersList(identityHelper.CurrentUserId, active, role, search, searchAllWords, sortBy, descSort, page, usersPerPage);
            return Ok(new UserResponse
            {
                Users = pagedUsersList.Users.Select(u => new UserDTO
                {
                    UserId = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Phone = u.Phone,
                    MobilePhone = u.MobilePhone,
                    Company = u.Company,
                    Department = u.Department,
                    Role = u.Roles.First().RoleId == roleService.AdminRoleId ? "Admin" : "User",
                    Active = u.Active,
                    LastActivity = u.LastActivity != null ? ((DateTime)u.LastActivity).ToString("yyyy-MM-dd HH:mm") : "Never",
                    TicketsCount = u.CreatedTickets.Union(u.RequestedTickets).Distinct(new Ticket.TicketsComparer()).Count()
                }),
                NumberOfPages = pagedUsersList.NumberOfPages,
                FoundItemsCount = pagedUsersList.FoundItemsCount,
                TotalItemsCount = pagedUsersList.TotalItemsCount
            });
        }
    }
}

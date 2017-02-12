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
        //private readonly IIdentityHelper identityHelper;
        //private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;

        public UsersController(IUnitOfWork unitOfWork, IIdentityHelper identityHelper)
        {
            //this.unitOfWork = unitOfWork;
            //this.identityHelper = identityHelper;
            userService = new UserService(unitOfWork, HttpContext.Current.User.Identity.GetUserId());
        }

        [HttpGet]
        [OverrideAuthorization]
        [Authorize]
        public IHttpActionResult GetUsers(bool? active = null, string role = null, string search = null, bool searchAllWords = false, string sortBy = "Last name", bool descSort = false, int page = 0, int? usersPerPage = null)
        {
            // todo: use RoleService instead of UnitOfWork
            string adminRoleId = unitOfWork.RoleRepository.Get(new Expression<Func<Role, bool>>[] { r => r.Name == "Admin" }).First().Id;
            PagedUsersList pagedUsersList = userService.GetPagedUsersList(active, role, search, searchAllWords, sortBy, descSort, page, usersPerPage);
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
                    Role = u.Roles.FirstOrDefault().RoleId == adminRoleId ? "Admin" : "User",
                    Active = u.Active,
                    LastActivity = u.LastActivity != null ? ((DateTime)u.LastActivity).ToString("yyyy-MM-dd HH:mm") : "Never",
                    TicketsCount = u.CreatedTickets.Union(u.RequestedTickets).Distinct().Count()
                }),
                NumberOfPages = pagedUsersList.NumberOfPages,
                FoundItemsCount = pagedUsersList.FoundItemsCount,
                TotalItemsCount = pagedUsersList.TotalItemsCount
            });
        }
    }
}

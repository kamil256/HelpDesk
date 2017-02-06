using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HelpDesk.UI.Infrastructure.Abstract;

namespace HelpDesk.UI.Infrastructure.Concrete
{
    public class IdentityHelper : IIdentityHelper
    {
        public UserManager UserManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().GetUserManager<UserManager>();
            }
        }

        public RoleManager RoleManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().GetUserManager<RoleManager>();
            }
        }

        public string GetRoleId(string roleName)
        {
            return RoleManager.FindByName(roleName).Id;
        }

        private string adminRoleId = null;

        public string AdminRoleId
        {
            get
            {
                if (adminRoleId == null)
                    adminRoleId = GetRoleId("Admin");
                return adminRoleId;
            }
        }

        private User currentUser = null;

        public User CurrentUser
        {
            get
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    if (currentUser == null)
                    {
                        currentUser = UserManager.FindByName(HttpContext.Current.User.Identity.Name);
                        currentUser.LastActivity = DateTime.Now;
                        UserManager.Update(currentUser);
                    }
                }
                else
                    currentUser = null;
                return currentUser;
            }
        }

        private bool? isCurrentUserAnAdministrator = null;

        public bool IsCurrentUserAnAdministrator()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                if (isCurrentUserAnAdministrator == null)
                    isCurrentUserAnAdministrator = UserManager.IsInRole(CurrentUser.Id, "Admin");
            }
            else
                isCurrentUserAnAdministrator = null;
            return isCurrentUserAnAdministrator ?? false;
        }

        public bool IsUserAnAdministrator(string userId)
        {
            return UserManager.IsInRole(userId, "Admin");
        }

        public int UsersPerPageSettingOfCurrentUser
        {
            get
            {
                return CurrentUser.Settings.UsersPerPage;
            }
        }

        public int TotalUsersCount
        {
            get
            {
                return UserManager.Users.Count();
            }
        }
    }
}
using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.Infrastructure
{
    public class IdentityHelper
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

        public User CurrentUser
        {
            get
            {
                if (currentUser == null)
                    currentUser = UserManager.FindByName(HttpContext.Current.User.Identity.Name);
                return currentUser;
            }
        }

        public bool IsCurrentUserAnAdministrator()
        {
            if (isCurrentUserAnAdministrator == null)
                isCurrentUserAnAdministrator = UserManager.IsInRole(CurrentUser.Id, "Admin");
            return (bool)isCurrentUserAnAdministrator;
        }

        private User currentUser = null;
        private bool? isCurrentUserAnAdministrator = null;        
    }
}
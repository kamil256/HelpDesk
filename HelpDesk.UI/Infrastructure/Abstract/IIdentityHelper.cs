using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.UI.Infrastructure.Abstract
{
    public interface IIdentityHelper
    {
        // todo: remove UserManager and RoleManager from interface
        UserManager UserManager { get; }
        RoleManager RoleManager { get; }

        string GetRoleId(string roleName);
        string AdminRoleId { get; }
        User CurrentUser { get; }
        bool IsCurrentUserAnAdministrator();
        bool IsUserAnAdministrator(string userId);
        int UsersPerPageSettingOfCurrentUser { get; }
        int TotalUsersCount { get; }
    }
}

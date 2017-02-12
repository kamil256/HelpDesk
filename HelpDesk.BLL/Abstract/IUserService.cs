using HelpDesk.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.BLL.Abstract
{
    public interface IUserService
    {
        PagedUsersList GetPagedUsersList(string loggedInUserId, bool? active, string role, string search, bool searchAllWords, string sortBy, bool descSort, int? page, int? usersPerPage);
    }
}

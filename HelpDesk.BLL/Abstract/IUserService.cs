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
        //IEnumerable<User> GetUsers(bool? active = null, string role = null, string search = null, bool searchAllWords = false, string sortBy = "Last name", bool descSort = false, int page = 0, int? usersPerPage = null);
        PagedUsersList GetUsers(bool? active = null, string role = null, string search = null, bool searchAllWords = false, string sortBy = "Last name", bool descSort = false, int page = 0, int? usersPerPage = null);
    }
}

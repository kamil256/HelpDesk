using HelpDesk.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.BLL.Abstract
{
    public interface IRoleService
    {
        string AdminRoleId { get; }
    }
}

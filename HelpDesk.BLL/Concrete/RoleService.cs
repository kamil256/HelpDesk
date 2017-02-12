using HelpDesk.BLL.Abstract;
using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.BLL.Concrete
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public string AdminRoleId
        {
            get
            {
                return unitOfWork.RoleRepository.Get(new Expression<Func<Role, bool>>[] { r => r.Name == "Admin" }).First().Id;
            }
        }
    }
}

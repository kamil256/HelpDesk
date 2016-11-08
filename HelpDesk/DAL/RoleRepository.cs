using HelpDesk.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Data.Entity;

namespace HelpDesk.DAL
{
    public class RoleRepository// : IRepository<AppRole>
    {
        private readonly AppRoleManager roleManager = System.Web.HttpContext.Current.Request.GetOwinContext().GetUserManager<AppRoleManager>();
        
        public IEnumerable<AppRole> Get(IEnumerable<Expression<Func<AppRole, bool>>> filters = null, Func<IQueryable<AppRole>, IOrderedQueryable<AppRole>> orderBy = null, int skip = 0, int take = 0, string includeProperties = "")
        {
            IQueryable<AppRole> query = roleManager.Roles;
            if (filters != null)
                foreach (var filter in filters)
                    if (filter != null)
                        query = query.Where(filter);
            if (orderBy != null)
                query = orderBy(query);
            if (skip != 0)
                query = query.Skip(skip);
            if (take != 0)
                query = query.Take(take);
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(includeProperty);
            return query;
        }

        public AppRole GetById(string id)
        {
            return roleManager.Roles.SingleOrDefault(u => u.Id == id);
        }

        public void Insert(AppUser entity)
        {
            throw new NotImplementedException();
        }

        public void Update(AppUser entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(AppUser entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }        
    }
}
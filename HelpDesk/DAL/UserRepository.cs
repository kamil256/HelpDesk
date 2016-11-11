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
    public class UserRepository// : IRepository<AppUser>
    {
        private readonly AppUserManager userManager = System.Web.HttpContext.Current.Request.GetOwinContext().GetUserManager<AppUserManager>();
        private readonly AppRoleManager roleManager = System.Web.HttpContext.Current.Request.GetOwinContext().GetUserManager<AppRoleManager>();
        private AppUser currentUser = null;

        public async Task<AppUser> GetCurrentUser()
        {
            if (currentUser == null)
                currentUser = await userManager.FindByEmailAsync(System.Web.HttpContext.Current.User.Identity.Name);
            return currentUser;
        }

        public async Task<bool> IsAnAdmin(string userId)
        {
            return await userManager.IsInRoleAsync(userId, "Admin");

        }

        public async Task<bool> IsCurrentUserAnAdmin()
        {
            AppUser currentUser = await GetCurrentUser();
            return await IsAnAdmin(currentUser.Id);
        }

        public string GetRoleName(string roleId)
        {
            AppRole role = roleManager.Roles.Single(r => r.Id == roleId);
            return role.Name;
        }

        public IEnumerable<AppUser> Get(IEnumerable<Expression<Func<AppUser, bool>>> filters = null, Func<IQueryable<AppUser>, IOrderedQueryable<AppUser>> orderBy = null, int skip = 0, int take = 0, string includeProperties = "")
        {
            IQueryable<AppUser> query = userManager.Users;
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

        public AppUser GetById(string id)
        {
            return userManager.Users.SingleOrDefault(u => u.Id == id);
        }

        public void Insert(AppUser entity)
        {
            throw new NotImplementedException();
        }

        public void Update(AppUser entity)
        {
            //userManager.UpdateAsync();
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
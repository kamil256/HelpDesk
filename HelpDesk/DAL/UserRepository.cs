using HelpDesk.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace HelpDesk.DAL
{
    public class UserRepository
    {
        private HelpDeskContext context;

        public UserRepository(HelpDeskContext context)
        {
            this.context = context;
        }

        public IEnumerable<User> GetAll(Expression<Func<User, bool>> filter = null, Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null)
        {
            IQueryable<User> query = context.Users;
            if (filter != null)
                query = query.Where(filter);
            if (orderBy != null)
                query = orderBy(query);
            return query.ToList();
        }

        public User GetById(int id)
        {
            return context.Users.Find(id);
        }

        public void Insert(User entity)
        {
            context.Users.Add(entity);
        }

        public void Update(User entity)
        {
            context.Users.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateUserInfo(User entity)
        {
            context.Users.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
            context.Entry(entity).Property(e => e.Password).IsModified = false;
            context.Entry(entity).Property(e => e.Salt).IsModified = false;
        }

        public void UpdateUserPassword(User entity)
        {
            context.Users.Attach(entity);
            context.Entry(entity).Property(e => e.Password).IsModified = true;
            context.Entry(entity).Property(e => e.Salt).IsModified = true;
        }

        public void Delete(int id)
        {
            Delete(context.Users.Find(id));
        }

        public void Delete(User entity)
        {
            if (context.Entry(entity).State == EntityState.Deleted)
                context.Users.Attach(entity);
            context.Users.Remove(entity);
        }
    }
}
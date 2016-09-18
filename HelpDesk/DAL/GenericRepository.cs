using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace HelpDesk.DAL
{
    public class GenericRepository<T> where T : class
    {
        private DbContext context;
        private DbSet<T> dbSet;

        public GenericRepository(DbContext context)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
        }

        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "")
        {
            List<Expression<Func<T, bool>>> filters = new List<Expression<Func<T, bool>>>();
            if (filter != null)
                filters.Add(filter);
            return GetAll(filters, orderBy, includeProperties);
        }

        public virtual IEnumerable<T> GetAll(List<Expression<Func<T, bool>>> filters = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "")
        {
            IQueryable<T> query = dbSet;
            if (filters != null)
                foreach (var filter in filters)
                    if (filter != null)
                        query = query.Where(filter);
            if (orderBy != null)
                query = orderBy(query);
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(includeProperty);
            return query.ToList();
        }

        public virtual T GetById(int id)
        {
            return dbSet.Find(id);
        }

        public virtual void Insert(T entity)
        {
            dbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            dbSet.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(int id)
        {
            Delete(dbSet.Find(id));
        }

        public virtual void Delete(T entity)
        {
            if (context.Entry(entity).State == EntityState.Deleted)
                dbSet.Attach(entity);
            dbSet.Remove(entity);
        }
    }
}
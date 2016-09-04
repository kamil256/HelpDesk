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

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
                query = query.Where(filter);
            if (query != null)
                query = orderBy(query);
            return query.ToList();
        }

        public T GetById(int id)
        {
            return dbSet.Find(id);
        }

        public void Insert(T entity)
        {
            dbSet.Add(entity);
        }

        public void Update(T entity)
        {
            dbSet.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            Delete(dbSet.Find(id));
        }

        public void Delete(T entity)
        {
            if (context.Entry(entity).State == EntityState.Deleted)
                dbSet.Attach(entity);
            dbSet.Remove(entity);
        }
    }
}
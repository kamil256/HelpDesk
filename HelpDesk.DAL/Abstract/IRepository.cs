using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.DAL.Abstract
{
    public interface IRepository<T> where T: class
    {
        IEnumerable<T> Get(IEnumerable<Expression<Func<T, bool>>> filters = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int skip = 0, int take = 0, string includeProperties = "");
        int Count(Expression<Func<T, bool>> filter = null);
        T GetById(int id);
        T GetById(string id);
        void Insert(T entity);
        void Update(T entity);
        void Delete(int id);
        void Delete(T entity);
    }
}

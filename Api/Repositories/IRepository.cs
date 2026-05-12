
using System.Linq.Expressions;

namespace SistemaProdutos.Repositories
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T? Get(Expression<Func<T, bool>> predicate);
        T Update(T entity);
        T Create(T entity);
        T Delete(T entity);

    }
}

using SistemaProdutos.Context;
using SistemaProdutos.Models;

namespace SistemaProdutos.Repositories
{
    public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
    {
        private readonly AppDbContext _context;
        public CategoriaRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
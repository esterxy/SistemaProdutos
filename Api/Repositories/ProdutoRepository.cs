using SistemaProdutos.Context;
using SistemaProdutos.Models;

namespace SistemaProdutos.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly AppDbContext _context;

        public ProdutoRepository(AppDbContext context)
        {
            _context = context;
        }

        public Produto Create(Produto produto)
        {
            if (produto is null)
            {
                throw new ArgumentNullException(nameof(produto));
            }

            _context.Produtos.Add(produto);
            _context.SaveChanges();
            return (produto);
        }

        public bool Delete(int id)
        {
            var produto = _context.Produtos.Find(id);
            if (produto is not null)
            {
                _context.Produtos.Remove(produto);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public Produto GetProduto(int id)
        {
            return _context.Produtos.FirstOrDefault(c => c.ProdutoId == id);
        }


        public IQueryable<Produto> GetProdutos()
        {
            return _context.Produtos;
        }

        public bool Update(Produto produto)
        {
            if (produto is null)
            {
                throw new ArgumentNullException(nameof(produto));
            }

            if (_context.Produtos.Any(f => f.ProdutoId == produto.ProdutoId))
            {
                _context.Produtos.Update(produto);
                _context.SaveChanges();
                return true;


            }

            return false;

         
        }
    }
}

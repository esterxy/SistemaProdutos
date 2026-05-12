using SistemaProdutos.Models;

namespace SistemaProdutos.Repositories
{
    public interface IProdutoRepository : IRepository<Produto>
    {
        IEnumerable<Produto> GetProdutosPorCategoria(int id);
    }

}
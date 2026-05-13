using SistemaProdutos.Models;

namespace SistemaProdutos.Repositories
{
    public interface IPedidoRepository : IRepository<Pedido>
    {
        /// <summary>
        /// Busca um pedido com eager loading dos itens e seus respectivos produtos.
        /// </summary>
        Pedido? GetPedidoComItens(int id);

        /// <summary>
        /// Busca todos os pedidos com eager loading dos itens e produtos.
        /// </summary>
        IEnumerable<Pedido> GetTodosComItens();
    }
}

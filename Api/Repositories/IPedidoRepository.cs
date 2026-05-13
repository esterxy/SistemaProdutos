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

        /// <summary>
        /// Busca pedidos de um cliente específico.
        /// </summary>
        IEnumerable<Pedido> GetPedidosPorCliente(int clienteId);

        /// <summary>
        /// Busca pedidos filtrados por categoria (via itens→produto→categoriaId).
        /// Se clienteId for null, retorna de todos os clientes.
        /// </summary>
        IEnumerable<Pedido> GetPedidosFiltrados(int? clienteId, int? categoriaId);
    }
}

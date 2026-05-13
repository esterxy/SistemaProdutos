using SistemaProdutos.DTOs;

namespace SistemaProdutos.Services
{
    public interface IPedidoService
    {
        /// <summary>
        /// Cria um novo pedido. clienteId é null para admin.
        /// </summary>
        Task<PedidoRespostaDto> CriarPedidoAsync(CriarPedidoDto dto, int? clienteId);

        Task<PedidoRespostaDto?> ObterPedidoAsync(int id);

        /// <summary>
        /// Lista pedidos filtrados. clienteId=null → todos (admin).
        /// categoriaId filtra por categoria dos produtos.
        /// </summary>
        Task<IEnumerable<PedidoRespostaDto>> ObterPedidosFiltradosAsync(int? clienteId, int? categoriaId);

        /// <summary>
        /// Cancela um pedido e restaura estoque.
        /// Se clienteId não for null, valida que o pedido pertence ao cliente.
        /// </summary>
        Task<bool> CancelarPedidoAsync(int id, int? clienteId);
    }
}

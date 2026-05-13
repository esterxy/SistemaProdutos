using SistemaProdutos.DTOs;

namespace SistemaProdutos.Services
{
    /// <summary>
    /// Interface do serviço de pedidos — define o contrato da camada de negócios.
    /// </summary>
    public interface IPedidoService
    {
        /// <summary>
        /// Cria um novo pedido validando itens, calculando preços a partir do banco,
        /// verificando estoque, e persistindo a entidade completa.
        /// </summary>
        Task<PedidoRespostaDto> CriarPedidoAsync(CriarPedidoDto dto);

        /// <summary>
        /// Busca um pedido específico com todos os itens detalhados.
        /// </summary>
        Task<PedidoRespostaDto?> ObterPedidoAsync(int id);

        /// <summary>
        /// Lista todos os pedidos ordenados por data decrescente.
        /// </summary>
        Task<IEnumerable<PedidoRespostaDto>> ObterTodosPedidosAsync();
    }
}

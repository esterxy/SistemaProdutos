using SistemaProdutos.DTOs;
using SistemaProdutos.Models;
using SistemaProdutos.Repositories;

namespace SistemaProdutos.Services
{
    /// <summary>
    /// Serviço de pedidos — contém toda a regra de negócio para criação e consulta de pedidos.
    /// Responsabilidades:
    /// - Validar dados de entrada
    /// - Buscar produtos no banco para obter preços reais
    /// - Verificar estoque disponível
    /// - Calcular subtotais e valor total
    /// - Persistir pedido e itens via UnitOfWork
    /// - Mapear entidades para DTOs de resposta
    /// </summary>
    public class PedidoService : IPedidoService
    {
        private readonly IUnitOfWork _uof;
        private readonly ILogger<PedidoService> _logger;

        public PedidoService(IUnitOfWork uof, ILogger<PedidoService> logger)
        {
            _uof = uof;
            _logger = logger;
        }

        /// <summary>
        /// Cria um novo pedido a partir do DTO.
        /// 
        /// Fluxo:
        /// 1. Valida se há itens no pedido
        /// 2. Para cada item, busca o produto no banco
        /// 3. Valida existência do produto e estoque suficiente
        /// 4. Calcula preço unitário × quantidade
        /// 5. Soma todos os subtotais para o valor total
        /// 6. Desconta o estoque
        /// 7. Salva tudo atomicamente via UnitOfWork
        /// </summary>
        public async Task<PedidoRespostaDto> CriarPedidoAsync(CriarPedidoDto dto)
        {
            _logger.LogInformation("Iniciando criação de pedido para o cliente: {Cliente}", dto.NomeCliente);

            // Validação de entrada
            if (dto.Itens == null || dto.Itens.Count == 0)
            {
                throw new ArgumentException("O pedido deve conter pelo menos um item.");
            }

            var pedido = new Pedido
            {
                NomeCliente = dto.NomeCliente,
                DataPedido = DateTime.Now,
                Status = "Pendente"
            };

            decimal valorTotal = 0;

            foreach (var itemDto in dto.Itens)
            {
                // Busca o produto no banco — preço vem de lá, nunca do cliente
                var produto = _uof.ProdutoRepository.Get(p => p.ProdutoId == itemDto.ProdutoId);

                if (produto == null)
                {
                    throw new KeyNotFoundException(
                        $"Produto com ID {itemDto.ProdutoId} não encontrado.");
                }

                // Validação de estoque
                if (produto.Estoque < itemDto.Quantidade)
                {
                    throw new InvalidOperationException(
                        $"Estoque insuficiente para o produto '{produto.Nome}'. " +
                        $"Disponível: {produto.Estoque}, Solicitado: {itemDto.Quantidade}.");
                }

                // Cálculo do subtotal com preço buscado do banco
                decimal subTotal = produto.Preco * itemDto.Quantidade;

                var itemPedido = new ItemPedido
                {
                    ProdutoId = itemDto.ProdutoId,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = produto.Preco,
                    SubTotal = subTotal
                };

                pedido.Itens.Add(itemPedido);
                valorTotal += subTotal;

                // Desconta estoque
                produto.Estoque -= itemDto.Quantidade;
                _uof.ProdutoRepository.Update(produto);

                _logger.LogInformation(
                    "Item adicionado: {Produto} x{Qtd} = R${SubTotal:F2}",
                    produto.Nome, itemDto.Quantidade, subTotal);
            }

            pedido.ValorTotal = valorTotal;

            // Persiste pedido + itens atomicamente
            _uof.PedidoRepository.Create(pedido);
            await _uof.CommitAsync();

            _logger.LogInformation(
                "Pedido #{PedidoId} criado com sucesso. Total: R${Total:F2}",
                pedido.PedidoId, valorTotal);

            // Retorna DTO com dados completos (recarrega com Include)
            return MapearParaDto(pedido);
        }

        /// <summary>
        /// Busca um pedido por ID com eager loading dos itens e produtos.
        /// </summary>
        public Task<PedidoRespostaDto?> ObterPedidoAsync(int id)
        {
            _logger.LogInformation("Buscando pedido #{PedidoId}", id);

            var pedido = _uof.PedidoRepository.GetPedidoComItens(id);

            if (pedido == null)
            {
                _logger.LogWarning("Pedido #{PedidoId} não encontrado.", id);
                return Task.FromResult<PedidoRespostaDto?>(null);
            }

            return Task.FromResult<PedidoRespostaDto?>(MapearParaDto(pedido));
        }

        /// <summary>
        /// Lista todos os pedidos com eager loading.
        /// </summary>
        public Task<IEnumerable<PedidoRespostaDto>> ObterTodosPedidosAsync()
        {
            _logger.LogInformation("Listando todos os pedidos.");

            var pedidos = _uof.PedidoRepository.GetTodosComItens();

            var resultado = pedidos.Select(MapearParaDto);

            return Task.FromResult(resultado);
        }

        /// <summary>
        /// Mapeia uma entidade Pedido para o DTO de resposta.
        /// Centraliza a conversão para manter o DRY.
        /// </summary>
        private static PedidoRespostaDto MapearParaDto(Pedido pedido)
        {
            return new PedidoRespostaDto
            {
                PedidoId = pedido.PedidoId,
                NomeCliente = pedido.NomeCliente,
                DataPedido = pedido.DataPedido,
                Status = pedido.Status,
                ValorTotal = pedido.ValorTotal,
                Itens = pedido.Itens.Select(item => new ItemPedidoRespostaDto
                {
                    ItemPedidoId = item.ItemPedidoId,
                    ProdutoId = item.ProdutoId,
                    NomeProduto = item.Produto?.Nome ?? "Produto não encontrado",
                    ImagemUrl = item.Produto?.ImageUrl,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario,
                    SubTotal = item.SubTotal
                }).ToList()
            };
        }
    }
}

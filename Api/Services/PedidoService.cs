using QRCoder;
using SistemaProdutos.DTOs;
using SistemaProdutos.Models;
using SistemaProdutos.Repositories;

namespace SistemaProdutos.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IUnitOfWork _uof;
        private readonly ILogger<PedidoService> _logger;

        public PedidoService(IUnitOfWork uof, ILogger<PedidoService> logger)
        {
            _uof = uof;
            _logger = logger;
        }

        public async Task<PedidoRespostaDto> CriarPedidoAsync(CriarPedidoDto dto, int? clienteId)
        {
            _logger.LogInformation("Criando pedido para: {Cliente} (ClienteId={Id})", dto.NomeCliente, clienteId);

            if (dto.Itens == null || dto.Itens.Count == 0)
                throw new ArgumentException("O pedido deve conter pelo menos um item.");

            var pedido = new Pedido
            {
                NomeCliente = dto.NomeCliente,
                DataPedido = DateTime.Now,
                Status = "Pendente",
                ClienteId = clienteId
            };

            decimal valorTotal = 0;

            foreach (var itemDto in dto.Itens)
            {
                var produto = _uof.ProdutoRepository.Get(p => p.ProdutoId == itemDto.ProdutoId);

                if (produto == null)
                    throw new KeyNotFoundException($"Produto com ID {itemDto.ProdutoId} não encontrado.");

                if (produto.Estoque < itemDto.Quantidade)
                    throw new InvalidOperationException(
                        $"Estoque insuficiente para '{produto.Nome}'. Disponível: {produto.Estoque}, Solicitado: {itemDto.Quantidade}.");

                decimal subTotal = produto.Preco * itemDto.Quantidade;

                pedido.Itens.Add(new ItemPedido
                {
                    ProdutoId = itemDto.ProdutoId,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = produto.Preco,
                    SubTotal = subTotal
                });

                valorTotal += subTotal;
                produto.Estoque -= itemDto.Quantidade;
                _uof.ProdutoRepository.Update(produto);

                _logger.LogInformation("Item: {Produto} x{Qtd} = R${Sub:F2}", produto.Nome, itemDto.Quantidade, subTotal);
            }

            pedido.ValorTotal = valorTotal;
            pedido.QrCodeBase64 = GerarQrCodePix(pedido.NomeCliente, valorTotal);

            _uof.PedidoRepository.Create(pedido);
            await _uof.CommitAsync();

            _logger.LogInformation("Pedido #{Id} criado. Total: R${Total:F2}", pedido.PedidoId, valorTotal);
            return MapearParaDto(pedido);
        }

        public async Task<bool> CancelarPedidoAsync(int id, int? clienteId)
        {
            _logger.LogInformation("Cancelando pedido #{Id} (clienteId={CId})", id, clienteId);

            var pedido = _uof.PedidoRepository.GetPedidoComItens(id);
            if (pedido == null) return false;

            // Já cancelado?
            if (pedido.Status.StartsWith("Cancelado"))
            {
                _logger.LogWarning("Pedido #{Id} já está cancelado.", id);
                return false;
            }

            // Se for cliente, valida que o pedido é dele
            if (clienteId.HasValue && pedido.ClienteId != clienteId.Value)
            {
                _logger.LogWarning("Cliente {CId} tentou cancelar pedido #{Id} de outro cliente.", clienteId, id);
                return false;
            }

            // Restaura estoque
            foreach (var item in pedido.Itens)
            {
                var produto = _uof.ProdutoRepository.Get(p => p.ProdutoId == item.ProdutoId);
                if (produto != null)
                {
                    produto.Estoque += item.Quantidade;
                    _uof.ProdutoRepository.Update(produto);
                }
            }

            // Define status de acordo com quem cancelou
            pedido.Status = clienteId.HasValue ? "Cancelado pelo cliente" : "Cancelado pela loja";
            _uof.PedidoRepository.Update(pedido);
            await _uof.CommitAsync();

            _logger.LogInformation("Pedido #{Id} → {Status}", id, pedido.Status);
            return true;
        }

        public Task<PedidoRespostaDto?> ObterPedidoAsync(int id)
        {
            var pedido = _uof.PedidoRepository.GetPedidoComItens(id);
            return Task.FromResult(pedido == null ? null : (PedidoRespostaDto?)MapearParaDto(pedido));
        }

        public Task<IEnumerable<PedidoRespostaDto>> ObterPedidosFiltradosAsync(int? clienteId, int? categoriaId)
        {
            _logger.LogInformation("Listando pedidos (clienteId={CId}, categoriaId={Cat})", clienteId, categoriaId);

            var pedidos = _uof.PedidoRepository.GetPedidosFiltrados(clienteId, categoriaId);
            var resultado = pedidos.Select(MapearParaDto);
            return Task.FromResult(resultado);
        }

        private static string GerarQrCodePix(string nomeCliente, decimal valor)
        {
            var valorFmt = valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            var payload = $"00020126580014BR.GOV.BCB.PIX0136saborbrasa@pix.com.br520400005303986540{valorFmt}5802BR5913Sabor e Brasa6008Sao Paulo62070503***6304";

            using var qrGen = new QRCodeGenerator();
            var data = qrGen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new PngByteQRCode(data);
            return Convert.ToBase64String(qrCode.GetGraphic(8));
        }

        private static PedidoRespostaDto MapearParaDto(Pedido pedido)
        {
            return new PedidoRespostaDto
            {
                PedidoId = pedido.PedidoId,
                NomeCliente = pedido.NomeCliente,
                DataPedido = pedido.DataPedido,
                Status = pedido.Status,
                ValorTotal = pedido.ValorTotal,
                QrCodeBase64 = pedido.QrCodeBase64,
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

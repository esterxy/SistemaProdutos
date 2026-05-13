namespace SistemaProdutos.DTOs
{
    /// <summary>
    /// DTO de resposta para pedidos — contém dados completos incluindo valor total calculado.
    /// </summary>
    public class PedidoRespostaDto
    {
        public int PedidoId { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
        public DateTime DataPedido { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public string? QrCodeBase64 { get; set; }
        public List<ItemPedidoRespostaDto> Itens { get; set; } = new();
    }

    /// <summary>
    /// DTO de resposta para cada item do pedido — inclui nome do produto e cálculos.
    /// </summary>
    public class ItemPedidoRespostaDto
    {
        public int ItemPedidoId { get; set; }
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public string? ImagemUrl { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal SubTotal { get; set; }
    }
}

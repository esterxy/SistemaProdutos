using System.ComponentModel.DataAnnotations;

namespace SistemaProdutos.DTOs
{
    /// <summary>
    /// DTO para criação de um novo pedido.
    /// Contém apenas os dados mínimos necessários — IDs de produto e quantidades.
    /// </summary>
    public class CriarPedidoDto
    {
        [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre {2} e {1} caracteres.")]
        public string NomeCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "O pedido deve conter pelo menos um item.")]
        [MinLength(1, ErrorMessage = "O pedido deve conter pelo menos um item.")]
        public List<ItemPedidoDto> Itens { get; set; } = new();
    }

    /// <summary>
    /// DTO de um item dentro de um pedido — apenas ProdutoId e Quantidade.
    /// O preço é buscado no banco para evitar manipulação pelo cliente.
    /// </summary>
    public class ItemPedidoDto
    {
        [Required(ErrorMessage = "O ID do produto é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "O ID do produto deve ser válido.")]
        public int ProdutoId { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória.")]
        [Range(1, 100, ErrorMessage = "A quantidade deve ser entre {1} e {2}.")]
        public int Quantidade { get; set; }
    }
}

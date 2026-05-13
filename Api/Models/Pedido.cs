using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaProdutos.Models
{
    [Table("Pedidos")]
    public class Pedido
    {
        public Pedido()
        {
            Itens = new List<ItemPedido>();
        }

        [Key]
        public int PedidoId { get; set; }

        [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")]
        public string NomeCliente { get; set; } = string.Empty;

        public DateTime DataPedido { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorTotal { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pendente";

        public ICollection<ItemPedido> Itens { get; set; }
    }
}

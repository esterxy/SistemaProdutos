using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SistemaProdutos.Models
{
    [Table("ItensPedido")]
    public class ItemPedido
    {
        [Key]
        public int ItemPedidoId { get; set; }

        [Required]
        public int Quantidade { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecoUnitario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal SubTotal { get; set; }

        // FK → Pedido
        public int PedidoId { get; set; }

        [JsonIgnore]
        public Pedido? Pedido { get; set; }

        // FK → Produto
        public int ProdutoId { get; set; }

        public Produto? Produto { get; set; }
    }
}

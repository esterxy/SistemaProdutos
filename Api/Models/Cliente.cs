using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaProdutos.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório.")]
        [StringLength(150, ErrorMessage = "O email deve ter no máximo {1} caracteres.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string SenhaHash { get; set; } = string.Empty;

        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}

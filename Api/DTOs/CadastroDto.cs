using System.ComponentModel.DataAnnotations;

namespace SistemaProdutos.DTOs
{
    /// <summary>
    /// DTO de entrada para cadastro de novo cliente.
    /// </summary>
    public class CadastroDto
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre {2} e {1} caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        [StringLength(150, ErrorMessage = "O email deve ter no máximo {1} caracteres.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "A senha deve ter entre {2} e {1} caracteres.")]
        public string Senha { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO de resposta após cadastro bem-sucedido.
    /// </summary>
    public class CadastroRespostaDto
    {
        public int ClienteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiracao { get; set; }
    }
}

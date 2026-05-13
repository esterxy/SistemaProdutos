using System.ComponentModel.DataAnnotations;

namespace SistemaProdutos.DTOs
{
    /// <summary>
    /// DTO de entrada para autenticação.
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "O usuário é obrigatório.")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO de resposta após login bem-sucedido — contém o token JWT.
    /// </summary>
    public class LoginRespostaDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiracao { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}

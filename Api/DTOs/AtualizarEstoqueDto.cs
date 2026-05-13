using System.ComponentModel.DataAnnotations;

namespace SistemaProdutos.DTOs
{
    /// <summary>
    /// DTO para atualização de estoque pelo admin.
    /// </summary>
    public class AtualizarEstoqueDto
    {
        [Required(ErrorMessage = "O estoque é obrigatório.")]
        [Range(0, 100000, ErrorMessage = "O estoque deve ser entre {1} e {2}.")]
        public float Estoque { get; set; }
    }
}

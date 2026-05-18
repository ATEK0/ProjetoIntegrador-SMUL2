using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class CreateChallengeViewModel
    {
        [Required(ErrorMessage = "O título do desafio é obrigatório.")]
        [StringLength(150, ErrorMessage = "O título do desafio não pode exceder 150 caracteres.")]
        public string Title { get; set; }

        public string Description { get; set; }

        public int? ClassId { get; set; }
    }
}

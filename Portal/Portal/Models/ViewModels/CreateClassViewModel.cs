using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class CreateClassViewModel
    {
        [Required(ErrorMessage = "O nome da turma é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome da turma não pode exceder 100 caracteres.")]
        public string Name { get; set; }
    }
}

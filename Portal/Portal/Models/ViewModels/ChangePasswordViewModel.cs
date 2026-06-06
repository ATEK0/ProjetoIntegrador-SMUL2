using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "A password atual é obrigatória.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova password é obrigatória.")]
        [MinLength(6, ErrorMessage = "A nova password deve ter pelo menos 6 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a nova password.")]
        [Compare("NewPassword", ErrorMessage = "A confirmação da password não coincide.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}

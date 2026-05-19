using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "A nova password deve ter pelo menos 6 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "A confirmação da password não coincide.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(255, ErrorMessage = "O nome não pode ter mais de 255 caracteres.")]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Insira um e-mail válido.")]
        [MaxLength(255, ErrorMessage = "O e-mail não pode ter mais de 255 caracteres.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        // Campos apenas para apresentação
        [Display(Name = "Função")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Conta criada em")]
        public DateTime CreatedAt { get; set; }
    }
}

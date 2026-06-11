using System;
using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(255, ErrorMessage = "O nome não pode ter mais de 255 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Insira um e-mail válido.")]
        [MaxLength(255, ErrorMessage = "O e-mail não pode ter mais de 255 caracteres.")]
        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public int? GenderId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

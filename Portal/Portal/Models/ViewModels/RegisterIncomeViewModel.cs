using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Portal.Models.ViewModels
{
    public class RegisterIncomeViewModel
    {
        [Required(ErrorMessage = "Seleciona um cenário.")]
        [Display(Name = "Cenário")]
        public int ScenarioId { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        [MaxLength(255)]
        [Display(Name = "Categoria")]
        public string Category { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser positivo.")]
        [Display(Name = "Valor (€)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "O mês é obrigatório.")]
        [Range(1, 12, ErrorMessage = "Mês inválido.")]
        [Display(Name = "Mês")]
        public int EntryMonth { get; set; }

        [Required(ErrorMessage = "A recorrência é obrigatória.")]
        [Display(Name = "Recorrência")]
        public RecurrenceType Recurrence { get; set; }

        // Lista de cenários disponíveis para preencher o <select>
        public List<SelectListItem> AvailableScenarios { get; set; } = new List<SelectListItem>();
    }
}

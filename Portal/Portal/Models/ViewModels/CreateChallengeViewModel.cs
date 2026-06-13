using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class CreateChallengeQuestionViewModel
    {
        [Required(ErrorMessage = "O texto da pergunta é obrigatório.")]
        public string QuestionText { get; set; } = string.Empty;

        public QuestionType QuestionType { get; set; }

        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? CorrectAnswer { get; set; }
    }

    public class CreateChallengeViewModel
    {
        [Required(ErrorMessage = "O título do desafio é obrigatório.")]
        [StringLength(150, ErrorMessage = "O título do desafio não pode exceder 150 caracteres.")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? ClassId { get; set; }

        public List<CreateChallengeQuestionViewModel> Questions { get; set; } = new();
    }
}

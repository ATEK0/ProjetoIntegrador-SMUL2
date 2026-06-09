using System.ComponentModel.DataAnnotations;
using Portal.Models.Enums;

namespace Portal.Models.ViewModels
{
    public class CreateQuizViewModel
    {
        [Required(ErrorMessage = "O título do desafio é obrigatório.")]
        [StringLength(150, ErrorMessage = "O título do desafio não pode exceder 150 caracteres.")]
        public string Title { get; set; }

        public string Description { get; set; }

        public int? ClassId { get; set; }

        [Required(ErrorMessage = "É necessário pelo menos uma pergunta.")]
        public List<QuizQuestionViewModel> Questions { get; set; } = new List<QuizQuestionViewModel>();
    }

    public class QuizQuestionViewModel
    {
        [Required(ErrorMessage = "O texto da pergunta é obrigatório.")]
        public string Text { get; set; }

        public int Order { get; set; }

        [Required(ErrorMessage = "É necessário pelo menos duas opções.")]
        public List<QuizOptionViewModel> Options { get; set; } = new List<QuizOptionViewModel>();
    }

    public class QuizOptionViewModel
    {
        [Required(ErrorMessage = "O texto da opção é obrigatório.")]
        public string Text { get; set; }

        public bool IsCorrect { get; set; }
    }

    public class SolveQuizViewModel
    {
        public int ChallengeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        
        public List<SolveQuizQuestionViewModel> Questions { get; set; } = new List<SolveQuizQuestionViewModel>();
    }

    public class SolveQuizQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public int Order { get; set; }

        public List<SolveQuizOptionViewModel> Options { get; set; } = new List<SolveQuizOptionViewModel>();
    }

    public class SolveQuizOptionViewModel
    {
        public int OptionId { get; set; }
        public string Text { get; set; }
    }

    public class SubmitQuizViewModel
    {
        [Required]
        public int ChallengeId { get; set; }

        [Required]
        public List<SubmitQuizAnswerViewModel> Answers { get; set; } = new List<SubmitQuizAnswerViewModel>();
    }

    public class SubmitQuizAnswerViewModel
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }
}

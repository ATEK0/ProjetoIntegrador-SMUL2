using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class QuizOption : BaseEntity
    {
        public int QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public QuizQuestion Question { get; set; }

        [Required]
        public string Text { get; set; }

        public bool IsCorrect { get; set; }

        public QuizOption(int questionId, string text, bool isCorrect)
        {
            QuestionId = questionId;
            Text = text;
            IsCorrect = isCorrect;
        }

        protected QuizOption() { }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class QuizAnswer : BaseEntity
    {
        public int SubmissionId { get; set; }

        [ForeignKey(nameof(SubmissionId))]
        public QuizSubmission Submission { get; set; }

        public int QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public QuizQuestion Question { get; set; }

        public int SelectedOptionId { get; set; }

        [ForeignKey(nameof(SelectedOptionId))]
        public QuizOption SelectedOption { get; set; }

        public QuizAnswer(int submissionId, int questionId, int selectedOptionId)
        {
            SubmissionId = submissionId;
            QuestionId = questionId;
            SelectedOptionId = selectedOptionId;
        }

        protected QuizAnswer() { }
    }
}

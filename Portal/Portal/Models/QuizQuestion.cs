using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class QuizQuestion : BaseEntity
    {
        public int ChallengeId { get; set; }

        [ForeignKey(nameof(ChallengeId))]
        public Challenge Challenge { get; set; }

        [Required]
        public string Text { get; set; }

        public int Order { get; set; }

        public ICollection<QuizOption> Options { get; set; }

        public QuizQuestion(int challengeId, string text, int order)
        {
            ChallengeId = challengeId;
            Text = text;
            Order = order;
        }

        protected QuizQuestion() { }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class QuizSubmission : BaseEntity
    {
        public int ChallengeId { get; set; }

        [ForeignKey(nameof(ChallengeId))]
        public Challenge Challenge { get; set; }

        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public User Student { get; set; }

        public int Score { get; set; }

        public ICollection<QuizAnswer> Answers { get; set; }

        public QuizSubmission(int challengeId, int studentId, int score)
        {
            ChallengeId = challengeId;
            StudentId = studentId;
            Score = score;
        }

        protected QuizSubmission() { }
    }
}

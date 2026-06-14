using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class ChallengeAnswer : BaseEntity
    {
        private int _challengeSubmissionId;
        private int _challengeQuestionId;
        private string _answerText = string.Empty;
        private bool? _isCorrect;

        public int ChallengeSubmissionId
        {
            get { return _challengeSubmissionId; }
            set { _challengeSubmissionId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(ChallengeSubmissionId))]
        public ChallengeSubmission ChallengeSubmission { get; set; } = null!;

        public int ChallengeQuestionId
        {
            get { return _challengeQuestionId; }
            set { _challengeQuestionId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(ChallengeQuestionId))]
        public ChallengeQuestion ChallengeQuestion { get; set; } = null!;

        [Required]
        public string AnswerText
        {
            get { return _answerText; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("O texto da resposta é obrigatório.");
                _answerText = value.Trim();
                UpdateTimestamp();
            }
        }

        public bool? IsCorrect
        {
            get { return _isCorrect; }
            set { _isCorrect = value; UpdateTimestamp(); }
        }

        public ChallengeAnswer(int challengeSubmissionId, int challengeQuestionId, string answerText)
        {
            ChallengeSubmissionId = challengeSubmissionId;
            ChallengeQuestionId = challengeQuestionId;
            AnswerText = answerText;
        }

        protected ChallengeAnswer() { }
    }
}

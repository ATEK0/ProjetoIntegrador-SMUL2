using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class ChallengeQuestion : BaseEntity
    {
        private int _challengeId;
        private string _questionText = string.Empty;
        private QuestionType _questionType;
        private string? _optionA;
        private string? _optionB;
        private string? _optionC;
        private string? _optionD;
        private string? _correctAnswer;

        public int ChallengeId
        {
            get { return _challengeId; }
            set { _challengeId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(ChallengeId))]
        public Challenge Challenge { get; set; } = null!;

        [Required]
        public string QuestionText
        {
            get { return _questionText; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("O texto da pergunta é obrigatório.");
                _questionText = value.Trim();
                UpdateTimestamp();
            }
        }

        public QuestionType QuestionType
        {
            get { return _questionType; }
            set { _questionType = value; UpdateTimestamp(); }
        }

        [MaxLength(255)]
        public string? OptionA
        {
            get { return _optionA; }
            set { _optionA = value?.Trim(); UpdateTimestamp(); }
        }

        [MaxLength(255)]
        public string? OptionB
        {
            get { return _optionB; }
            set { _optionB = value?.Trim(); UpdateTimestamp(); }
        }

        [MaxLength(255)]
        public string? OptionC
        {
            get { return _optionC; }
            set { _optionC = value?.Trim(); UpdateTimestamp(); }
        }

        [MaxLength(255)]
        public string? OptionD
        {
            get { return _optionD; }
            set { _optionD = value?.Trim(); UpdateTimestamp(); }
        }

        [MaxLength(255)]
        public string? CorrectAnswer
        {
            get { return _correctAnswer; }
            set { _correctAnswer = value?.Trim(); UpdateTimestamp(); }
        }

        public ChallengeQuestion(int challengeId, string questionText, QuestionType questionType)
        {
            ChallengeId = challengeId;
            QuestionText = questionText;
            QuestionType = questionType;
        }

        protected ChallengeQuestion() { }
    }
}

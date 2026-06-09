using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Portal.Models.Enums;

namespace Portal.Models
{
    public class Challenge : BaseEntity
    {
        private int _teacherId;
        private int? _classId;
        private string _title;
        private string _description;
        private string _accessLinkCode;

        public int TeacherId
        {
            get { return _teacherId; }
            set { _teacherId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(TeacherId))]
        public User Teacher { get; set; }

        public ChallengeType Type { get; set; } = ChallengeType.Standard;

        public ICollection<Scenario> Scenarios { get; set; }
        public ICollection<QuizQuestion> QuizQuestions { get; set; }
        public ICollection<QuizSubmission> Submissions { get; set; }
        public ICollection<StudentChallenge> StudentChallenges { get; set; }

        [NotMapped]
        public int ParticipantsCount => ClassId.HasValue 
            ? (Class?.Enrollments?.Count ?? 0) 
            : (StudentChallenges?.Count ?? 0);

        public int? ClassId
        {
            get { return _classId; }
            set { _classId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(ClassId))]
        public SchoolClass Class { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title
        {
            get { return _title; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("O título é obrigatório.");
                _title = value.Trim();
                UpdateTimestamp();
            }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value?.Trim(); UpdateTimestamp(); }
        }

        [Required]
        [MaxLength(255)]
        public string AccessLinkCode
        {
            get { return _accessLinkCode; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("O código de acesso é obrigatório.");
                _accessLinkCode = value.Trim();
                UpdateTimestamp();
            }
        }

        public Challenge(int teacherId, string title, string accessLinkCode, ChallengeType type = ChallengeType.Standard)
        {
            TeacherId = teacherId;
            Title = title;
            AccessLinkCode = accessLinkCode;
            Type = type;
        }

        protected Challenge() { }
    }
}
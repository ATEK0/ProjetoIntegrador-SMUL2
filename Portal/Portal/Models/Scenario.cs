using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class Scenario : BaseEntity
    {
        private int _studentId;
        private int? _challengeId;
        private string _familyName;
        private decimal _initialBalance;

        public int StudentId
        {
            get { return _studentId; }
            set { _studentId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(StudentId))]
        public User Student { get; set; }

        public int? ChallengeId
        {
            get { return _challengeId; }
            set { _challengeId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(ChallengeId))]
        public Challenge Challenge { get; set; }

        public List<Entry> Entries { get; set; } = new();

        [Required]
        [MaxLength(255)]
        public string FamilyName
        {
            get { return _familyName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("O nome de família é obrigatório.");
                _familyName = value.Trim();
                UpdateTimestamp();
            }
        }

        public decimal InitialBalance
        {
            get { return _initialBalance; }
            set { _initialBalance = value; UpdateTimestamp(); }
        }

        public Scenario(int studentId, string familyName, decimal initialBalance)
        {
            StudentId = studentId;
            FamilyName = familyName;
            InitialBalance = initialBalance;
        }

        protected Scenario() { }
    }
}

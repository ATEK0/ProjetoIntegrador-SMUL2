using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class Objective : BaseEntity
    {
        private int _scenarioId;
        private string _description;
        private decimal _targetValue;
        private int _termMonths;

        public int ScenarioId
        {
            get { return _scenarioId; }
            set { _scenarioId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(ScenarioId))]
        public Scenario Scenario { get; set; }

        [Required]
        [MaxLength(255)]
        public string Description
        {
            get { return _description; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A descrição é obrigatória.");
                _description = value.Trim();
                UpdateTimestamp();
            }
        }

        public decimal TargetValue
        {
            get { return _targetValue; }
            set { _targetValue = value; UpdateTimestamp(); }
        }

        public int TermMonths
        {
            get { return _termMonths; }
            set { _termMonths = value; UpdateTimestamp(); }
        }

        public Objective(int scenarioId, string description, decimal targetValue, int termMonths)
        {
            ScenarioId = scenarioId;
            Description = description;
            TargetValue = targetValue;
            TermMonths = termMonths;
        }

        protected Objective() { }
    }
}

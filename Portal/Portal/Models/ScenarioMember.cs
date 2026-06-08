using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class ScenarioMember : BaseEntity
    {
        private int _scenarioId;
        private string _name;
        private decimal _monthlyIncome;

        public int ScenarioId
        {
            get { return _scenarioId; }
            set { _scenarioId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(ScenarioId))]
        public Scenario Scenario { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name
        {
            get { return _name; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("O nome é obrigatório.");
                _name = value.Trim();
                UpdateTimestamp();
            }
        }

        public decimal MonthlyIncome
        {
            get { return _monthlyIncome; }
            set { _monthlyIncome = value; UpdateTimestamp(); }
        }

        public ScenarioMember(int scenarioId, string name, decimal monthlyIncome)
        {
            ScenarioId = scenarioId;
            Name = name;
            MonthlyIncome = monthlyIncome;
        }

        protected ScenarioMember() { }
    }
}

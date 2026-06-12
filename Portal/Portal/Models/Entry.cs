using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class Entry : BaseEntity
    {
        private int _scenarioId;
        private EntryType _entryType;
        private string _category;
        private decimal _amount;
        private int _entryMonth;
        private RecurrenceType _recurrence;

        public int ScenarioId
        {
            get { return _scenarioId; }
            set { _scenarioId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(ScenarioId))]
        public Scenario Scenario { get; set; }

        public EntryType EntryType
        {
            get { return _entryType; }
            set { _entryType = value; UpdateTimestamp(); }
        }

        [Required]
        [MaxLength(255)]
        public string Category
        {
            get { return _category; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A categoria é obrigatória.");
                _category = value.Trim();
                UpdateTimestamp();
            }
        }

        public decimal Amount
        {
            get { return _amount; }
            set { _amount = value; UpdateTimestamp(); }
        }

        public int EntryMonth
        {
            get { return _entryMonth; }
            set { _entryMonth = value; UpdateTimestamp(); }
        }

        public RecurrenceType Recurrence
        {
            get { return _recurrence; }
            set { _recurrence = value; UpdateTimestamp(); }
        }

        public Entry(int scenarioId, EntryType entryType, string category, decimal amount, int entryMonth, RecurrenceType recurrence)
        {
            ScenarioId = scenarioId;
            EntryType = entryType;
            Category = category;
            Amount = amount;
            EntryMonth = entryMonth;
            Recurrence = recurrence;
        }

        protected Entry() { }
    }
}

using System.Collections.Generic;
using Portal.Models;

namespace Portal.Models.ViewModels
{
    public class ScenarioDetailsViewModel
    {
        public Scenario Scenario { get; set; }
        public List<Entry> Incomes { get; set; } = new List<Entry>();
        public List<Entry> Expenses { get; set; } = new List<Entry>();

        public int? SelectedMonth { get; set; }
        public List<ScenarioMember> Members { get; set; } = new List<ScenarioMember>();

        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance { get; set; }
        public decimal SavingsRate { get; set; }
        public decimal FinalBankBalance { get; set; }
    }
}

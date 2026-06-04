using System.Collections.Generic;
using Portal.Models;

namespace Portal.Models.ViewModels
{
    public class ScenarioDetailsViewModel
    {
        public Scenario Scenario { get; set; }
        public List<Entry> Incomes { get; set; } = new List<Entry>();
        public List<Entry> Expenses { get; set; } = new List<Entry>();
    }
}

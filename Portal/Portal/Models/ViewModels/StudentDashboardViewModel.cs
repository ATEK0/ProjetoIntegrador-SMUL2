using System.Collections.Generic;
using Portal.Models;

namespace Portal.Models.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; }
        public List<Scenario> Scenarios { get; set; } = new List<Scenario>();
        public List<Entry> RecentIncomes { get; set; } = new List<Entry>();
        public decimal TotalMonthlyIncome { get; set; }
    }
}

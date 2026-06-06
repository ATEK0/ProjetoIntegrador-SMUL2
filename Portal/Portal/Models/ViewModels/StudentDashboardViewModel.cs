using System.Collections.Generic;
using Portal.Models;

namespace Portal.Models.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public List<Scenario> Scenarios { get; set; } = new List<Scenario>();
        public List<UserClassEnrollmentDetail> EnrolledClasses { get; set; } = new List<UserClassEnrollmentDetail>();
        public List<Challenge> PendingChallenges { get; set; } = new List<Challenge>();
    }
}

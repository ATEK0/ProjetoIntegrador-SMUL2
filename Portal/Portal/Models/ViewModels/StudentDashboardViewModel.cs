using System;
using System.Collections.Generic;
using Portal.Models;

namespace Portal.Models.ViewModels
{
    public class StudentChallengeViewModel
    {
        public Challenge Challenge { get; set; } = null!;
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int? SubmissionId { get; set; }
        public bool IsGraded { get; set; }
    }

    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public List<Scenario> Scenarios { get; set; } = new List<Scenario>();
        public List<UserClassEnrollmentDetail> EnrolledClasses { get; set; } = new List<UserClassEnrollmentDetail>();
        public List<StudentChallengeViewModel> PendingChallenges { get; set; } = new List<StudentChallengeViewModel>();
    }
}

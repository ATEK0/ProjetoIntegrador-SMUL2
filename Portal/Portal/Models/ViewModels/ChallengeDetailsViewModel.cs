using System;
using System.Collections.Generic;
using Portal.Models;

namespace Portal.Models.ViewModels
{
    public class StudentSubmissionStatusViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool IsGraded { get; set; }
        public DateTime? GradedAt { get; set; }
        public int? SubmissionId { get; set; }
    }

    public class ChallengeDetailsViewModel
    {
        public Challenge Challenge { get; set; } = null!;
        public List<ChallengeQuestion> Questions { get; set; } = new();
        public List<StudentSubmissionStatusViewModel> StudentStatuses { get; set; } = new();
        public List<SchoolClass> AvailableClasses { get; set; } = new();
    }
}

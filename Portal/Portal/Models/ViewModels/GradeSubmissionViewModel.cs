using System.Collections.Generic;
using Portal.Models;

namespace Portal.Models.ViewModels
{
    public class GradeSubmissionViewModel
    {
        public ChallengeSubmission Submission { get; set; } = null!;
        public List<ChallengeQuestion> Questions { get; set; } = new();
    }
}

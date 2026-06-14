using Portal.Models;
using System;
using System.Collections.Generic;

namespace Portal.Models.ViewModels
{
    public class StudentChallengeStatusViewModel
    {
        public int ChallengeId { get; set; }
        public string ChallengeTitle { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsGraded { get; set; }
        public int? SubmissionId { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    public class ClassDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MembershipCode { get; set; }
        public int TeacherId { get; set; }
        public User Teacher { get; set; }
        public List<User> EnrolledStudents { get; set; } = new List<User>();
        public List<User> AvailableTeachers { get; set; } = new List<User>();
        public bool CanEdit { get; set; }
        public List<Challenge> Challenges { get; set; } = new List<Challenge>();
        public Dictionary<int, List<StudentChallengeStatusViewModel>> StudentChallengeStatuses { get; set; } = new();
    }
}

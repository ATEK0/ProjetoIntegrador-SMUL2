using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class ChallengeSubmission : BaseEntity
    {
        private int _studentId;
        private int _challengeId;
        private string? _feedback;
        private DateTime? _gradedAt;

        public int StudentId
        {
            get { return _studentId; }
            set { _studentId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(StudentId))]
        public User Student { get; set; } = null!;

        public int ChallengeId
        {
            get { return _challengeId; }
            set { _challengeId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(ChallengeId))]
        public Challenge Challenge { get; set; } = null!;

        public string? Feedback
        {
            get { return _feedback; }
            set { _feedback = value?.Trim(); UpdateTimestamp(); }
        }

        public DateTime? GradedAt
        {
            get { return _gradedAt; }
            set { _gradedAt = value; UpdateTimestamp(); }
        }

        public List<ChallengeAnswer> Answers { get; set; } = new();

        public ChallengeSubmission(int studentId, int challengeId)
        {
            StudentId = studentId;
            ChallengeId = challengeId;
        }

        protected ChallengeSubmission() { }
    }
}

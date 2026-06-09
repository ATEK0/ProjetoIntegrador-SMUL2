using System;

namespace Portal.Models
{
    public class StudentChallenge
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ChallengeId { get; set; }
        public DateTime EnrolledAt { get; set; }

        public virtual User Student { get; set; }
        public virtual Challenge Challenge { get; set; }

        protected StudentChallenge() { }

        public StudentChallenge(int studentId, int challengeId)
        {
            StudentId = studentId;
            ChallengeId = challengeId;
            EnrolledAt = DateTime.Now;
        }
    }
}

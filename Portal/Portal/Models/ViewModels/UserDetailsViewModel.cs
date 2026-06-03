using Portal.Models;
using System.Collections.Generic;

namespace Portal.Models.ViewModels
{
    public class UserDetailsViewModel
    {
        public User User { get; set; }
        public List<SchoolClass> TaughtClasses { get; set; } = new List<SchoolClass>();
        public List<SchoolClass> EnrolledClasses { get; set; } = new List<SchoolClass>();
        public List<Challenge> CreatedChallenges { get; set; } = new List<Challenge>();
        public List<Challenge> ParticipatedChallenges { get; set; } = new List<Challenge>();
        public List<Role> AvailableRoles { get; set; } = new List<Role>();
    }
}

using Portal.Models;
using System.Collections.Generic;

namespace Portal.Models.ViewModels
{
    public class ChallengeDetailsViewModel
    {
        public Challenge Challenge { get; set; }
        public List<Scenario> Scenarios { get; set; } = new List<Scenario>();
        public List<SchoolClass> AvailableClasses { get; set; } = new List<SchoolClass>();
    }
}

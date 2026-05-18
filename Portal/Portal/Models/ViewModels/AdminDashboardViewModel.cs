using System.Collections.Generic;

namespace Portal.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public IEnumerable<SchoolClass> Classes { get; set; }
        public IEnumerable<Challenge> Challenges { get; set; }
    }
}

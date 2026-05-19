using System.Collections.Generic;
using Portal.Models;

namespace Portal.Models.ViewModels
{
    public class TeacherDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalChallenges { get; set; }
        public List<TeacherClassDetailViewModel> Classes { get; set; } = new();
        public List<Challenge> Challenges { get; set; } = new();
    }

    public class TeacherClassDetailViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string MembershipCode { get; set; }
        public int StudentCount { get; set; }
    }
}

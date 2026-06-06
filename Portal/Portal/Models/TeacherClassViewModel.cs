using System.Collections.Generic;

namespace Portal.Models.ViewModels
{
    public class TeacherClassViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string MembershipCode { get; set; }
        public List<string> StudentNames { get; set; }
    }
}
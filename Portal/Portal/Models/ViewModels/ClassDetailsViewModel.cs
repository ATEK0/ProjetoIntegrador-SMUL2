using Portal.Models;
using System.Collections.Generic;

namespace Portal.Models.ViewModels
{
    public class ClassDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MembershipCode { get; set; }
        public int TeacherId { get; set; }
        public User Teacher { get; set; }
        public List<User> EnrolledStudents { get; set; } = new List<User>();
        public List<User> AvailableTeachers { get; set; } = new List<User>();
    }
}

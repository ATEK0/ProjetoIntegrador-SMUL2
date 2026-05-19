using System.Collections.Generic;
using Portal.Models;

namespace Portal.Models.ViewModels
{
    public class AdminUsersViewModel
    {
        public List<AdminUserDetailViewModel> Users { get; set; } = new();
        public List<SchoolClass> Classes { get; set; } = new();
    }

    public class AdminUserDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public List<UserClassEnrollmentDetail> Enrollments { get; set; } = new();
    }

    public class UserClassEnrollmentDetail
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
    }
}

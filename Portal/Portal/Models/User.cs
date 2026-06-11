using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class User : BaseEntity
    {
        private string _name;
        private int? _genderId;
        private DateTime? _birthDate;
        private string _email;
        private string _passwordHash;
        private int _userStatusId;
        private int _roleId;

        [Required]
        [MaxLength(255)]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = string.IsNullOrWhiteSpace(value) ? "Anónimo" : value.Trim();
                UpdateTimestamp();
            }
        }

        public int? GenderId
        {
            get { return _genderId; }
            set { _genderId = value; UpdateTimestamp(); }
        }

        public DateTime? BirthDate
        {
            get { return _birthDate; }
            set { _birthDate = value; UpdateTimestamp(); }
        }

        [MaxLength(255)]
        public string Email
        {
            get { return _email; }
            set
            {
                _email = string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLower();
                UpdateTimestamp();
            }
        }

        [MaxLength(255)]
        public string PasswordHash
        {
            get { return _passwordHash; }
            set { _passwordHash = value; UpdateTimestamp(); }
        }

        public int UserStatusId
        {
            get { return _userStatusId; }
            set { _userStatusId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(UserStatusId))]
        public UserStatus UserStatus { get; set; }

        public int RoleId
        {
            get { return _roleId; }
            set { _roleId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }

        public List<SchoolClass> TaughtClasses { get; set; } = new();
        public List<ClassEnrollment> Enrollments { get; set; } = new();
        public List<Challenge> CreatedChallenges { get; set; } = new();
        public List<Scenario> Scenarios { get; set; } = new();

        public User(string name, int userStatusId, int roleId)
        {
            Name = name;
            UserStatusId = userStatusId;
            RoleId = roleId;
        }

        protected User() { }
    }
}

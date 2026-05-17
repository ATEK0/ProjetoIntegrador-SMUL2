using System.ComponentModel.DataAnnotations;

namespace Portal.Models
{
    public class UserStatus : BaseEntity
    {
        private string _statusName;
        private readonly List<User> _users = new();

        [Required]
        [MaxLength(255)]
        public string StatusName
        {
            get { return _statusName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("O nome do status não pode estar vazio.");

                _statusName = value.Trim();
                UpdateTimestamp();
            }
        }

        public IReadOnlyCollection<User> Users => _users.AsReadOnly();

        public UserStatus(string statusName)
        {
            StatusName = statusName;
        }

        protected UserStatus() { }
    }
}
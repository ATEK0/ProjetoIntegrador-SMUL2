using System.ComponentModel.DataAnnotations;

namespace Portal.Models
{
    public class UserStatus : BaseEntity
    {
        private string _statusName;

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

        public List<User> Users { get; set; } = new();

        public UserStatus(string statusName)
        {
            StatusName = statusName;
        }

        protected UserStatus() { }
    }
}

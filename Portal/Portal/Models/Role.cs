using System.ComponentModel.DataAnnotations;

namespace Portal.Models
{
    public class Role : BaseEntity
    {
        private string _roleName;

        [Required]
        [MaxLength(255)]
        public string RoleName
        {
            get { return _roleName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("O nome da Role não pode estar vazio.");

                _roleName = value.Trim();
                UpdateTimestamp();
            }
        }

        public List<User> Users { get; set; } = new();

        public Role(string roleName)
        {
            RoleName = roleName;
        }

        protected Role() { }
    }
}

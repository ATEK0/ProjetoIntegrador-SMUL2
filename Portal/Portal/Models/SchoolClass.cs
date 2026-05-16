using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    [Table("classes")]
    public class SchoolClass : BaseEntity
    {
        private int _teacherId;
        private string _name;
        private string _membershipCode;

        public int TeacherId
        {
            get { return _teacherId; }
            set { _teacherId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(TeacherId))]
        public User Teacher { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name
        {
            get { return _name; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("O nome é obrigatório.");
                _name = value.Trim();
                UpdateTimestamp();
            }
        }

        [Required]
        [MaxLength(255)]
        public string MembershipCode
        {
            get { return _membershipCode; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("O código é obrigatório.");
                _membershipCode = value.Trim();
                UpdateTimestamp();
            }
        }

        public SchoolClass(int teacherId, string name, string membershipCode)
        {
            TeacherId = teacherId;
            Name = name;
            MembershipCode = membershipCode;
        }

        protected SchoolClass() { }
    }
}
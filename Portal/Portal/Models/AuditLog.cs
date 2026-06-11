using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    [Table("audit_logs")]
    public class AuditLog
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int? UserId { get; set; }

        [MaxLength(255)]
        public string? UserEmail { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("action")]
        public string ActionName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = null!;

        [MaxLength(50)]
        public string? EntityId { get; set; }
    }
}

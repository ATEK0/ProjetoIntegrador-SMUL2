using System.ComponentModel.DataAnnotations;

namespace Portal.Models
{
    public abstract class BaseEntity
    {
        private int _id;
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime _updatedAt = DateTime.UtcNow;
        private DateTime? _deletedAt;

        [Key]
        public int Id
        {
            get { return _id; }
            protected set { _id = value; }
        }

        public DateTime CreatedAt
        {
            get { return _createdAt; }
            protected set { _createdAt = value; }
        }

        public DateTime UpdatedAt
        {
            get { return _updatedAt; }
            protected set { _updatedAt = value; }
        }

        public DateTime? DeletedAt
        {
            get { return _deletedAt; }
            protected set { _deletedAt = value; }
        }

        public void MarkAsDeleted()
        {
            _deletedAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        protected void UpdateTimestamp()
        {
            _updatedAt = DateTime.UtcNow;
        }
    }
}

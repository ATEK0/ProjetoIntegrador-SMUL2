using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Models
{
    public class ClassEnrollment : BaseEntity
    {
        private int _classId;
        private int _studentId;

        public int ClassId
        {
            get { return _classId; }
            set { _classId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(ClassId))]
        public SchoolClass Class { get; set; }

        public int StudentId
        {
            get { return _studentId; }
            set { _studentId = value; UpdateTimestamp(); }
        }

        [ForeignKey(nameof(StudentId))]
        public User Student { get; set; }

        public ClassEnrollment(int classId, int studentId)
        {
            ClassId = classId;
            StudentId = studentId;
        }
        protected ClassEnrollment() { }

    }
}
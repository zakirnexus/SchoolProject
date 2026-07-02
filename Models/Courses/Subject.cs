using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models.Courses
{
    [Table("tb_subjects")]
    public class Subject
    {
        [Key]
        [Column("subject_id")]
        public int SubjectId { get; set; }

        [Column("subject_name")]
        public string SubjectName { get; set; } = string.Empty;

        [Column("subject_slug")]
        public string SubjectSlug { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }
    }
}
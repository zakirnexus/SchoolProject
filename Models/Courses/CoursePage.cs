using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models.Courses
{
    [Table("tb_course_pages")]
    public class CoursePage
    {
        [Key]
        [Column("page_id")]
        public int PageId { get; set; }

        [Column("course_id")]
        public int CourseId { get; set; }

        [Column("specialization_id")]
        public int? SpecializationId { get; set; }

        [Column("page_slug")]
        public string? PageSlug { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        public virtual Course? Course { get; set; }
        public virtual Specialization? Specialization { get; set; }
    }
}
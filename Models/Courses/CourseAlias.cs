using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models.Courses
{
    [Table("tb_course_aliases")]
    public class CourseAlias
    {
        [Key]
        [Column("alias_id")]
        public int AliasId { get; set; }

        [Column("course_id")]
        public int CourseId { get; set; }

        [Column("alias_name")]
        public string AliasName { get; set; } = "";

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
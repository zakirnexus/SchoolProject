using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models.Courses
{
    [Table("tb_degree_types")]
    public class DegreeType
    {
        [Key]
        [Column("degree_type_id")]
        public int DegreeTypeId { get; set; }

        [Column("degree_name")]
        public string DegreeName { get; set; } = string.Empty;

        [Column("degree_slug")]
        public string DegreeSlug { get; set; } = string.Empty;

        [Column("level_id")]
        public int? LevelId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
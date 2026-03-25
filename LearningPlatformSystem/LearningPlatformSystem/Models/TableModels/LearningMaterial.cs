using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningPlatformSystem.Models.TableModels
{
    public class LearningMaterial
    {
        [Key]
        public int MaterialId { get; set; }

        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Difficulty { get; set; } = string.Empty;

        //العلاقاات

        public int CourseCode { get; set; }

        [ForeignKey("CourseCode")]
        public Course ?Course { get; set; }
    }
}

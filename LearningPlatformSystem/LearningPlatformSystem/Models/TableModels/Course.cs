using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningPlatformSystem.Models.TableModels
{
    public class Course
    {
        [Key]
        public int CourseCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(50)]
        public string RequiredLevel { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int? Hours { get; set; }

        [MaxLength(500)]
        public string CourseObjective { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Language { get; set; } = string.Empty;

        [Required]
        public DateTime CreationDate { get; set; }

        //العلاقات
        //to learners
        public List<Learner> ?Learners { get; set; }

        //to tutors
        public int TutorId { get; set; }

        [ForeignKey("TutorId")]
        public Tutor ?Tutor { get; set; } 
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningPlatformSystem.Models.TableModels
{
    public class AwardedCertificate
    {
        [Key]
        public int CertificateSerialCode { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime AwardDate { get; set; }

        //العلاقات
        public int LearnerId { get; set; }

        [ForeignKey("LearnerId")]
        public Learner ?Learner { get; set; }

        public int CourseCode { get; set; }

        [ForeignKey("CourseCode")]
        public Course ?Course { get; set; }
    }
}

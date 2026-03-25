using LearningPlatformSystem.Models.TableModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningPlatformSystem.Models.CoreModels
{
    public class EnrollmentRequest
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int LearnerId { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false;
        public string? RejectionReason { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public bool IsRejected { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        [ForeignKey("LearnerId")]
        public Learner? Learner { get; set; }

    }
}

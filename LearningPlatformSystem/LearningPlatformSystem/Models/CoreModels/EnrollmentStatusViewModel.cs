namespace LearningPlatformSystem.Models.CoreModels
{
    public class EnrollmentStatusViewModel
    {
        public int CourseId { get; set; }
        public string ?CourseTitle { get; set; }
        public string ?Status { get; set; } 
        public string ?RejectionReason { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
    }
}

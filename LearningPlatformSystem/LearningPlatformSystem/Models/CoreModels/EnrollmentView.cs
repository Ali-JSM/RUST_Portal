using LearningPlatformSystem.Models.TableModels;

namespace LearningPlatformSystem.Models.CoreModels
{
    public class EnrollmentView
    {
        public int? SelectedCourseId { get; set; }
        public int? SelectedLearnerId { get; set; }
        public List<Course> ?AvailableCourses { get; set; } 
        public List<Learner> ?AvailableLearners { get; set; }
        public List<EnrollmentRecord>? CurrentEnrollments { get; set; }
    }
}

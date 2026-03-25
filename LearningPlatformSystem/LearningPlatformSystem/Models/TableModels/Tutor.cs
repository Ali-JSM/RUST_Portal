using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningPlatformSystem.Models.TableModels
{
    public class Tutor
    {
        [Key]
        public int TutorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Major { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Rating { get; set; } = string.Empty;

        [Required]
        public DateTime SignUpDate { get; set; }

        public string OtherInfo { get; set; } = string.Empty;
        public string? imagePath { get; set; }

        [NotMapped]
        public IFormFile clientFile { get; set; }

        //العلاقات
        public List<Course> ?Courses { get; set; }
    }
}

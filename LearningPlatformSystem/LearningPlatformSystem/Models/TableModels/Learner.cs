using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningPlatformSystem.Models.TableModels
{
    public class Learner
    {
        [Key]
        public int LearnerId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Major { get; set; } = string.Empty;

        [Required]
        public DateTime SignUpDate { get; set; }
        public string? imagePath { get; set; } = "Admin (2).png";

        [NotMapped]
        public IFormFile clientFile { get; set; }
        //العلاقات
        public List<Course> ?Courses { get; set; }
        public List<AwardedCertificate> ?AwardedCertificates { get; set; }
    }

}


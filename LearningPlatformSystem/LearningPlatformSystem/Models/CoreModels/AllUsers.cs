using System.ComponentModel.DataAnnotations;

namespace LearningPlatformSystem.Models.CoreModels
{
    public class AllUsers
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Sign Up Date")]
        public DateTime SignUpDate { get; set; }

        [Display(Name = "Roles")]
        public List<string> Roles { get; set; } = new List<string>();

        public List<string> AllRoles { get; set; } = new List<string>();
    }
}

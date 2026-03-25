using Microsoft.AspNetCore.Identity;

namespace LearningPlatformSystem.Models.CoreModels
{
    public class AuthUsers : IdentityUser
    {
        //اليوزر + الايميل موجودين بالكلاس الاب IdentityUser
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public DateTime SignUpDate { get; set; }

        public string Role = string.Empty;
    }
}

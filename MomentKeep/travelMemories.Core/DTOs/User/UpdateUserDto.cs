using System.ComponentModel.DataAnnotations;

namespace TravelMemories.Core.DTOs.User
{
    public class UpdateUserDto
    {
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
    }
}
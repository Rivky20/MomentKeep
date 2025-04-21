using System.ComponentModel.DataAnnotations;

namespace TravelMemories.Core.DTOs.Admin
{
    public class UserManagementRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(20)]
        public string Role { get; set; }

        public int? StorageQuota { get; set; }

        public int? AiQuota { get; set; }

        [MinLength(8)]
        [MaxLength(30)]
        public string Password { get; set; }
    }
}
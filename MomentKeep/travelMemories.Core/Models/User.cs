namespace TravelMemories.Core.Models
{
    public class User : BaseEntity
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public int StorageQuota { get; set; } = 10240; // Default 10GB in MB
        public int AiQuota { get; set; } = 50; // Default 50 AI images per month

        // Navigation properties
        public virtual ICollection<Trip> Trips { get; set; }
        public virtual ICollection<Image> Images { get; set; }
    }
}
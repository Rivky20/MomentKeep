namespace TravelMemories.Core.DTOs.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public int StorageQuota { get; set; }
        public int AiQuota { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
namespace TravelMemories.Core.Models
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; }

        // Navigation properties
        public virtual ICollection<ImageTag> ImageTags { get; set; }
    }
}
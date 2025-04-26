namespace MomentKeep.Core.Models
{
    public class ImageTag
    {
        public Guid ImageId { get; set; }
        public Guid TagId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }

        // Navigation properties
        public virtual Image Image { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
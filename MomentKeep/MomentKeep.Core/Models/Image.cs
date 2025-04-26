using travelMemories.Core.Models;

namespace MomentKeep.Core.Models
{
    public class Image : BaseEntity
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int FileSize { get; set; }
        public string MimeType { get; set; }
        public DateTime? TakenAt { get; set; }
        public Guid? TripId { get; set; }
        public bool IsAiGenerated { get; set; } = false;
        public string AiPrompt { get; set; }
        public string AiStyle { get; set; }
        public Guid UserId { get; set; }

        // Navigation properties
        public virtual Folder Trip { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<ImageTag> ImageTags { get; set; }
    }
}
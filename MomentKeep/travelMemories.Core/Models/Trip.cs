namespace TravelMemories.Core.Models
{
    public class Trip : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid UserId { get; set; }
        public Guid? ShareId { get; set; } = Guid.NewGuid();
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string LocationName { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<Image> Images { get; set; }
    }
}
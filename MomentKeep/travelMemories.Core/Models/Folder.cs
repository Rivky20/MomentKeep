using System;
using System.Collections.Generic;
using TravelMemories.Core.Models;

namespace travelMemories.Core.Models
{
    public class Folder : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid UserId { get; set; }
        public Guid? ShareId { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<Image> Images { get; set; } = new List<Image>();
    }
}
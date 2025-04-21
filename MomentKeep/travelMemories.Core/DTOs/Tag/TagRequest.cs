using System.ComponentModel.DataAnnotations;

namespace TravelMemories.Core.DTOs.Tag
{
    public class TagRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
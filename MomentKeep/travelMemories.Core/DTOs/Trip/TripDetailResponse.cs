using TravelMemories.Core.DTOs.Image;

namespace TravelMemories.Core.DTOs.Trip
{
    public class TripDetailResponse : TripResponse
    {
        public IEnumerable<ImageResponse> Images { get; set; }
    }
}
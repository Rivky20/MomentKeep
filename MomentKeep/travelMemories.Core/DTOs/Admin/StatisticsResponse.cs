namespace TravelMemories.Core.DTOs.Admin
{
    public class StatisticsResponse
    {
        public int TotalUsers { get; set; }
        public int TotalTrips { get; set; }
        public int TotalImages { get; set; }
        public int TotalAiImages { get; set; }
        public int TotalStorageUsedMB { get; set; }
        public IEnumerable<UserActivityItem> RecentUserActivity { get; set; }
        public Dictionary<string, int> UsersByMonth { get; set; }
        public Dictionary<string, int> TripsByMonth { get; set; }
        public Dictionary<string, int> ImagesByMonth { get; set; }
    }
}
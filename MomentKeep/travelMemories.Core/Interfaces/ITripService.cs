using TravelMemories.Core.DTOs.Trip;
using TravelMemories.Core.Models;

namespace TravelMemories.Core.Interfaces
{

    public interface ITripService
    {
        Task<Trip> CreateTripAsync(Guid userId, TripRequest tripRequest);
        Task<Trip> UpdateTripAsync(Guid tripId, Guid userId, TripRequest tripRequest);
        Task<bool> DeleteTripAsync(Guid tripId, Guid userId);
        Task<Trip> GetTripByIdAsync(Guid tripId, Guid userId);
        Task<Trip> GetTripByShareIdAsync(Guid shareId);
        Task<IEnumerable<Trip>> GetUserTripsAsync(Guid userId);
        Task<Trip> GetTripWithImagesAsync(Guid tripId, Guid userId);
        Task<Guid> RegenerateShareIdAsync(Guid tripId, Guid userId);
    }
}
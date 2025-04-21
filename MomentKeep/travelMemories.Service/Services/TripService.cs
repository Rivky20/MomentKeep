using TravelMemories.Core.DTOs.Trip;
using TravelMemories.Core.Interfaces.Repositories;
using TravelMemories.Core.Interfaces;
using TravelMemories.Core.Models;

namespace TravelMemories.Service.Services
{
    public class TripService : ITripService
    {
        private readonly ITripRepository _tripRepository;

        public TripService(ITripRepository tripRepository)
        {
            _tripRepository = tripRepository;
        }

        public async Task<Trip> CreateTripAsync(Guid userId, TripRequest tripRequest)
        {
            var trip = new Trip
            {
                Name = tripRequest.Name,
                Description = tripRequest.Description,
                StartDate = tripRequest.StartDate,
                EndDate = tripRequest.EndDate,
                UserId = userId,
                ShareId = Guid.NewGuid(),
                LocationName = tripRequest.LocationName,
                Latitude = tripRequest.Latitude,
                Longitude = tripRequest.Longitude,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _tripRepository.AddAsync(trip);
            await _tripRepository.SaveChangesAsync();

            return trip;
        }

        public async Task<Trip> UpdateTripAsync(Guid tripId, Guid userId, TripRequest tripRequest)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId);

            if (trip == null)
            {
                throw new KeyNotFoundException("Trip not found");
            }

            if (trip.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this trip");
            }

            // Update trip properties
            trip.Name = tripRequest.Name;
            trip.Description = tripRequest.Description;
            trip.StartDate = tripRequest.StartDate;
            trip.EndDate = tripRequest.EndDate;
            trip.LocationName = tripRequest.LocationName;
            trip.Latitude = tripRequest.Latitude;
            trip.Longitude = tripRequest.Longitude;
            trip.UpdatedAt = DateTime.UtcNow;
            trip.UpdatedBy = userId;

            _tripRepository.Update(trip);
            await _tripRepository.SaveChangesAsync();

            return trip;
        }

        public async Task<bool> DeleteTripAsync(Guid tripId, Guid userId)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId);

            if (trip == null)
            {
                return false;
            }

            if (trip.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this trip");
            }

            _tripRepository.Remove(trip);
            await _tripRepository.SaveChangesAsync();

            return true;
        }

        public async Task<Trip> GetTripByIdAsync(Guid tripId, Guid userId)
        {
            var trip = await _tripRepository.GetTripWithDetailsAsync(tripId);

            if (trip == null)
            {
                return null;
            }

            if (trip.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this trip");
            }

            return trip;
        }

        public async Task<Trip> GetTripByShareIdAsync(Guid shareId)
        {
            return await _tripRepository.GetTripByShareIdAsync(shareId);
        }

        public async Task<IEnumerable<Trip>> GetUserTripsAsync(Guid userId)
        {
            return await _tripRepository.GetUserTripsAsync(userId);
        }

        public async Task<Trip> GetTripWithImagesAsync(Guid tripId, Guid userId)
        {
            var trip = await _tripRepository.GetTripWithImagesAsync(tripId);

            if (trip == null)
            {
                return null;
            }

            if (trip.UserId != userId && trip.ShareId == null)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this trip");
            }

            return trip;
        }

        public async Task<Guid> RegenerateShareIdAsync(Guid tripId, Guid userId)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId);

            if (trip == null)
            {
                throw new KeyNotFoundException("Trip not found");
            }

            if (trip.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this trip");
            }

            trip.ShareId = Guid.NewGuid();
            trip.UpdatedAt = DateTime.UtcNow;
            trip.UpdatedBy = userId;

            _tripRepository.Update(trip);
            await _tripRepository.SaveChangesAsync();

            return trip.ShareId.Value;
        }
    }
}
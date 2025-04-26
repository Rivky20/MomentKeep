using Microsoft.EntityFrameworkCore;
using TravelMemories.Core.Interfaces.Repositories;
using TravelMemories.Core.Models;
using TravelMemories.Data.Context;

namespace TravelMemories.Data.Repositories
{
    public class TripRepository : Repository<Folder>, IFolderRepository
    {
        public TripRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Folder> GetTripWithDetailsAsync(Guid tripId)
        {
            return await _context.Trips
                .Include(t => t.User)
                .Include(t => t.Images)
                .SingleOrDefaultAsync(t => t.Id == tripId);
        }

        public async Task<Folder> GetTripByShareIdAsync(Guid shareId)
        {
            return await _context.Trips
                .Include(t => t.Images)
                .SingleOrDefaultAsync(t => t.ShareId == shareId);
        }

        public async Task<IEnumerable<Folder>> GetUserTripsAsync(Guid userId)
        {
            return await _context.Trips
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
        }

        public async Task<Folder> GetTripWithImagesAsync(Guid tripId)
        {
            return await _context.Trips
                .Include(t => t.Images)
                .ThenInclude(i => i.ImageTags)
                .ThenInclude(it => it.Tag)
                .SingleOrDefaultAsync(t => t.Id == tripId);
        }
    }
}
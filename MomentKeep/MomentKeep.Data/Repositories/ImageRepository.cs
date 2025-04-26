using Microsoft.EntityFrameworkCore;
using MomentKeep.Core.Interfaces.Repositories;
using MomentKeep.Core.Models;
using MomentKeep.Data.Context;

namespace MomentKeep.Data.Repositories
{
    public class ImageRepository : Repository<Image>, IImageRepository
    {
        public ImageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Image> GetImageWithDetailsAsync(Guid imageId)
        {
            return await _context.Images
                .Include(i => i.Trip)
                .Include(i => i.User)
                .Include(i => i.ImageTags)
                .ThenInclude(it => it.Tag)
                .SingleOrDefaultAsync(i => i.Id == imageId);
        }

        public async Task<IEnumerable<Image>> GetImagesByTripAsync(Guid tripId)
        {
            Console.WriteLine($"DEBUG: Fetching images for trip ID: {tripId}");
            var images = await _context.Images
                .Include(i => i.ImageTags)
                .ThenInclude(it => it.Tag)
                .Where(i => i.TripId == tripId)
                .OrderBy(i => i.TakenAt)
                .ToListAsync();

            Console.WriteLine($"DEBUG: Found {images.Count()} images for trip ID: {tripId}");
            foreach (var img in images)
            {
                Console.WriteLine($"DEBUG: Image {img.Id} - TripId: {img.TripId}");
            }

            return images;
        }

        public async Task<IEnumerable<Image>> GetImagesByUserAsync(Guid userId)
        {
            return await _context.Images
                .Include(i => i.Trip)
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Image>> SearchImagesAsync(Guid? tripId, Guid? userId, string[] tags, DateTime? dateFrom, DateTime? dateTo, string location)
        {
            var query = _context.Images.AsQueryable();

            if (tripId.HasValue)
            {
                query = query.Where(i => i.TripId == tripId);
            }

            if (userId.HasValue)
            {
                query = query.Where(i => i.UserId == userId);
            }

            if (tags != null && tags.Length > 0)
            {
                query = query.Where(i => i.ImageTags.Any(it => tags.Contains(it.Tag.Name)));
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(i => i.TakenAt >= dateFrom || i.CreatedAt >= dateFrom);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(i => i.TakenAt <= dateTo || i.CreatedAt <= dateTo);
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(i => i.Trip.LocationName.Contains(location));
            }

            return await query
                .Include(i => i.Trip)
                .Include(i => i.ImageTags)
                .ThenInclude(it => it.Tag)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Image>> GetAiGeneratedImagesAsync(Guid userId)
        {
            return await _context.Images
                .Where(i => i.UserId == userId && i.IsAiGenerated)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }
    }
}
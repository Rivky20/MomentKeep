using MomentKeep.Core.Interfaces.Repositories;
using travelMemories.Core.Models;

public interface IFolderRepository : IRepository<Folder>
{
    Task<Folder> GetFolderWithImagesAsync(Guid id);
    Task<Folder> GetFolderByShareIdAsync(Guid shareId);
    Task<IEnumerable<Folder>> GetFoldersByUserIdAsync(Guid userId);
    Task<Folder> GetFolderWithDetailsAsync(Guid folderId);
    Task<IEnumerable<Folder>> GetUserFoldersAsync(Guid userId);
}
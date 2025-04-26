using MomentKeep.Core.DTOs.Folder;
using MomentKeep.Core.Interfaces.Repositories;
using MomentKeep.Core.Interfaces;
using MomentKeep.Core.Models;
using travelMemories.Core.Models;

namespace MomentKeep.Service.Services
{
    public class FolderService : IFolderService
    {
        private readonly IFolderRepository _folderRepository;

        public FolderService(IFolderRepository folderRepository)
        {
            _folderRepository = folderRepository;
        }

        public async Task<Folder> CreateFolderAsync(Guid userId, FolderRequest folderRequest)
        {
            var folder = new Folder
            {
                Name = folderRequest.Name,
                Description = folderRequest.Description,
                UserId = userId,
                ShareId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _folderRepository.AddAsync(folder);
            await _folderRepository.SaveChangesAsync();

            return folder;
        }

        public async Task<Folder> UpdateFolderAsync(Guid folderId, Guid userId, FolderRequest folderRequest)
        {
            var folder = await _folderRepository.GetByIdAsync(folderId);

            if (folder == null)
            {
                throw new KeyNotFoundException("Folder not found");
            }

            if (folder.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this folder");
            }

            // Update folder properties
            folder.Name = folderRequest.Name;
            folder.Description = folderRequest.Description;
            folder.UpdatedAt = DateTime.UtcNow;
            folder.UpdatedBy = userId;

            _folderRepository.Update(folder);
            await _folderRepository.SaveChangesAsync();

            return folder;
        }

        public async Task<bool> DeleteFolderAsync(Guid folderId, Guid userId)
        {
            var folder = await _folderRepository.GetByIdAsync(folderId);

            if (folder == null)
            {
                return false;
            }

            if (folder.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this folder");
            }

            _folderRepository.Remove(folder);
            await _folderRepository.SaveChangesAsync();

            return true;
        }

        public async Task<Folder> GetFolderByIdAsync(Guid folderId, Guid userId)
        {
            var folder = await _folderRepository.GetFolderWithDetailsAsync(folderId);

            if (folder == null)
            {
                return null;
            }

            if (folder.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this folder");
            }

            return folder;
        }

        public async Task<Folder> GetFolderByShareIdAsync(Guid shareId)
        {
            return await _folderRepository.GetFolderByShareIdAsync(shareId);
        }

        public async Task<IEnumerable<Folder>> GetUserFoldersAsync(Guid userId)
        {
            return await _folderRepository.GetUserFoldersAsync(userId);
        }

        public async Task<Folder> GetFolderWithImagesAsync(Guid folderId, Guid userId)
        {
            var folder = await _folderRepository.GetFolderWithImagesAsync(folderId);

            if (folder == null)
            {
                return null;
            }

            if (folder.UserId != userId && folder.ShareId == null)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this folder");
            }

            return folder;
        }

        public async Task<Guid> RegenerateShareIdAsync(Guid folderId, Guid userId)
        {
            var folder = await _folderRepository.GetByIdAsync(folderId);

            if (folder == null)
            {
                throw new KeyNotFoundException("Folder not found");
            }

            if (folder.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this folder");
            }

            folder.ShareId = Guid.NewGuid();
            folder.UpdatedAt = DateTime.UtcNow;
            folder.UpdatedBy = userId;

            _folderRepository.Update(folder);
            await _folderRepository.SaveChangesAsync();

            return folder.ShareId.Value;
        }

        public Task<IEnumerable<FolderResponse>> GetAllFoldersAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        Task<FolderDetailResponse> IFolderService.GetFolderByIdAsync(Guid id, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<FolderResponse> CreateFolderAsync(FolderRequest request, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<FolderResponse> UpdateFolderAsync(Guid id, FolderRequest request, Guid userId)
        {
            throw new NotImplementedException();
        }

        Task IFolderService.DeleteFolderAsync(Guid id, Guid userId)
        {
            return DeleteFolderAsync(id, userId);
        }

        public Task<string> ShareFolderAsync(Guid id, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<FolderDetailResponse> GetSharedFolderAsync(Guid shareId)
        {
            throw new NotImplementedException();
        }
    }
}
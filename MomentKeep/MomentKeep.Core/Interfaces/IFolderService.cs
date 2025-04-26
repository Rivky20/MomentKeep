public interface IFolderService
{
    Task<IEnumerable<FolderResponse>> GetAllFoldersAsync(Guid userId);
    Task<FolderDetailResponse> GetFolderByIdAsync(Guid id, Guid userId);
    Task<FolderResponse> CreateFolderAsync(FolderRequest request, Guid userId);
    Task<FolderResponse> UpdateFolderAsync(Guid id, FolderRequest request, Guid userId);
    Task DeleteFolderAsync(Guid id, Guid userId);
    Task<string> ShareFolderAsync(Guid id, Guid userId);
    Task<FolderDetailResponse> GetSharedFolderAsync(Guid shareId);
}
using EList.Common.Models;
using FileInfo = EList.FilestorageClient.Models.FileInfo;

namespace EList.FilestorageClient
{
    public interface IFilestorageClient
    {
        Task<CommandResult> RegisterAuthDataAsync(Guid userToken, Guid accountId, string JwtHash);
        Task<CommandResult> DisableAuthDataAsync(Guid userToken, string JwtHash);
        Task<CommandResult<FileInfo>> GetFileInfoAsync(Guid id, Guid userToken, string jwt);
        Task<CommandResult> DeleteFileAsync(Guid id, Guid userToken, string jwt);
        Task<CommandResult> SetFilesVisibilityAsync(IReadOnlyList<Guid> fileIds, FileVisibility visibility);
        Task<CommandResult> SetFilesAccessStatusAsync(IReadOnlyList<Guid> fileIds, FileAccessStatus accessStatus);
        /// <summary>
        /// Download file bytes via service-token (works for Blocked files; for staff proxy).
        /// </summary>
        Task<CommandResult<FileDownloadResult>> DownloadFileAsync(Guid fileId, bool? fullSize = null);
        Task<CommandResult<List<Guid>>> GetGcCandidateIdsAsync(int olderThanDays = 7, int take = 100);
        /// <summary>Delete via service-token only (orphan GC / internal cleanup).</summary>
        Task<CommandResult> DeleteFileAsServiceAsync(Guid id);
    }
}


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
    }
}

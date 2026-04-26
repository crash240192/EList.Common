using EList.Common.Models;

namespace EList.FilestorageClient
{
    public interface IFilestorageClient
    {
        Task<CommandResult> RegisterAuthDataAsync(Guid userToken, Guid accountId, string JwtHash);

        Task<CommandResult> DisableAuthDataAsync(Guid userToken, string JwtHash);
    }
}

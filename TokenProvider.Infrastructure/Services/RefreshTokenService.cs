using TokenProvider.Infrastructure.Models;

namespace TokenProvider.Infrastructure.Services;


public interface IRefreshTokenService
{
    Task<RefreshTokenResult> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}


public class RefreshTokenService : IRefreshTokenService
{

    #region GetRefreshTokenAsync
    public Task<RefreshTokenResult> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    #endregion
}

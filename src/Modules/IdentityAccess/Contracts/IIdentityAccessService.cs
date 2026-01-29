using Finitech.Modules.IdentityAccess.Contracts.DTOs;

namespace Finitech.Modules.IdentityAccess.Contracts;

public interface IIdentityAccessService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task ChangeContactAsync(Guid userId, ChangeContactRequest request, CancellationToken cancellationToken = default);
    Task LockAccountAsync(Guid userId, string reason, string? adminId = null, CancellationToken cancellationToken = default);
    Task UnlockAccountAsync(Guid userId, string? adminId = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Guid?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default);
}

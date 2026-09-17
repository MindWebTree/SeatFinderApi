using CounsellingApp.Domain.Entities;

namespace CounsellingApp.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetByIdAsync(Guid userId);

    /// <summary>Creates the user AND assigns them the given role (via UserRole) in one call.</summary>
    Task<Guid> CreateUserAsync(User user, Guid roleId);
    Task UpdatePasswordAsync(Guid userId, string newPasswordHash);

    /// <summary>Returns the internal int token_id (used later to mark the token as used).</summary>
    Task<int> CreatePasswordResetTokenAsync(Guid userId, string tokenHash, DateTime expiryDate);
    Task<PasswordResetToken?> GetPasswordResetTokenAsync(string tokenHash);
    Task MarkPasswordResetTokenUsedAsync(int tokenId);

    /// <summary>Returns the internal int refresh_token_id.</summary>
    Task<int> CreateRefreshTokenAsync(Guid userId, string token, DateTime expiryDate);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);



    Task<User?> GetByEmailOrPhoneAsync(string? email, string? phone);
    Task<Guid> CreateUserViaOtpAsync(string fullName, string? email, string? phone, int? stateId, Guid roleId);

    Task CreateOtpAsync(string identifier, string identifierType, string otpHash, DateTime expiryDate);
    Task<OtpVerification?> GetLatestOtpAsync(string identifier);
    Task MarkOtpUsedAsync(int otpId);
    Task IncrementOtpAttemptsAsync(int otpId);
}

using System.Data;
using CounsellingApp.Application.Interfaces;
using CounsellingApp.Domain.Entities;
using CounsellingApp.Infrastructure.Data;
using Dapper;

namespace CounsellingApp.Infrastructure.Repositories;


public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_Email", email);

        return await connection.QueryFirstOrDefaultAsync<User>(
            "USP_API_AUTH_GET_USER_BY_EMAIL", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_PhoneNumber", phoneNumber);

        return await connection.QueryFirstOrDefaultAsync<User>(
            "USP_API_AUTH_GET_USER_BY_PHONENUMBER", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_UserId", userId, DbType.Guid);

        return await connection.QueryFirstOrDefaultAsync<User>(
            "USP_API_AUTH_GET_USER_BY_USERID", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<Guid> CreateUserAsync(User user, Guid roleId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_FullName", user.FullName);
        parameters.Add("_Email", user.Email);
        parameters.Add("_PhoneNumber", user.PhoneNumber);
        parameters.Add("_PasswordHash", user.PasswordHash);
        parameters.Add("_RoleId", roleId, DbType.Guid);
        parameters.Add("_NeetRank", user.NeetRank);
        parameters.Add("_Category", user.Category);
        parameters.Add("_UserId", dbType: DbType.Guid, direction: ParameterDirection.Output, size: 36);

        await connection.ExecuteAsync("USP_API_AUTH_USER_REGISTER", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<Guid>("_UserId");
    }

    public async Task UpdatePasswordAsync(Guid userId, string newPasswordHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_UserId", userId, DbType.Guid);
        parameters.Add("_PasswordHash", newPasswordHash);

        await connection.ExecuteAsync("USP_API_AUTH_USER_UPDATE_PASSWORD", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> CreatePasswordResetTokenAsync(Guid userId, string tokenHash, DateTime expiryDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_UserId", userId, DbType.Guid);
        parameters.Add("_Token", tokenHash);
        parameters.Add("_ExpiryDate", expiryDate);
        parameters.Add("_TokenId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("USP_API_AUTH_CREATE_PASSWORD_RESET_TOKEN", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<int>("_TokenId");
    }

    public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string tokenHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_Token", tokenHash);

        return await connection.QueryFirstOrDefaultAsync<PasswordResetToken>(
            "USP_API_AUTH_GET_PASSWORD_RESET_TOKEN", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task MarkPasswordResetTokenUsedAsync(int tokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_TokenId", tokenId);

        await connection.ExecuteAsync("USP_API_AUTH_MARK_PASSWORD_RESET_TOKEN_USED", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> CreateRefreshTokenAsync(Guid userId, string token, DateTime expiryDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_UserId", userId, DbType.Guid);
        parameters.Add("_Token", token);
        parameters.Add("_ExpiryDate", expiryDate);
        parameters.Add("_RefreshTokenId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("USP_API_AUTH_CREATE_REFRESH_TOKEN", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<int>("_RefreshTokenId");
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_Token", token);

        return await connection.QueryFirstOrDefaultAsync<RefreshToken>(
            "USP_API_AUTH_GET_REFRESH_TOKEN", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_Token", token);

        await connection.ExecuteAsync("USP_API_AUTH_REVOKE_REFRESH_TOKEN", parameters, commandType: CommandType.StoredProcedure);
    }



    public async Task CreateOtpAsync(string identifier, string identifierType, string otpHash, DateTime expiryDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_Identifier", identifier);
        parameters.Add("_Identifiertype", identifierType);  
        parameters.Add("_OtpHash", otpHash);
        parameters.Add("_ExpiryDate", expiryDate);
        await connection.ExecuteAsync("USP_API_CREATE_OPT", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<OtpVerification?> GetLatestOtpAsync(string identifier)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_Identifier", identifier);
        return await connection.QueryFirstOrDefaultAsync<OtpVerification>(
            "USP_API_GET_OTP", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task MarkOtpUsedAsync(int otpId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_Id", otpId);
        await connection.ExecuteAsync("USP_API_MARK_OTP_USED", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task IncrementOtpAttemptsAsync(int otpId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_Id", otpId);
        await connection.ExecuteAsync("USP_API_INCREMENT_OTP_ATTEMPTS", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<User?> GetByEmailOrPhoneAsync(string? email, string? phone)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_Email", email);
        parameters.Add("_Phone", phone);
        return await connection.QueryFirstOrDefaultAsync<User>(
            "USP_API_GET_USER_BY_EMAIL_OR_PHONE", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<Guid> CreateUserViaOtpAsync(string fullName, string? email, string? phone, int? stateId, Guid roleId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_FullName", fullName);
        parameters.Add("_Email", email);
        parameters.Add("_Phone", phone);
        parameters.Add("_StateId", stateId);
        parameters.Add("_RoleId", roleId, DbType.Guid);
        parameters.Add("_UserId", dbType: DbType.Guid, direction: ParameterDirection.Output, size: 36);
        await connection.ExecuteAsync("USP_API_REGISTER_USER_VIA_OTP", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<Guid>("_UserId");
    }

    public async Task<bool> DeleteAccountAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var p = new DynamicParameters();
        p.Add("_UserId", userId.ToString());

        var deleted = await connection.ExecuteScalarAsync<int>(
            "USP_API_DELETE_ACCOUNT", p, commandType: CommandType.StoredProcedure);

        return deleted > 0;
    }
}

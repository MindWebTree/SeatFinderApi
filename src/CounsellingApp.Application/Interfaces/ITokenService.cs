using CounsellingApp.Domain.Entities;

namespace CounsellingApp.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}

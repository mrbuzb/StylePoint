using StylePoint.Domain.Entities;

namespace StylePoint.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddRefreshToken(RefreshToken refreshToken);
    Task<RefreshToken> SelectRefreshToken(string refreshToken, long userId);
    Task DeleteRefreshToken(string refreshToken);
}
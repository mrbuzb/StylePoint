using StylePoint.Domain.Entities;

namespace StylePoint.Application.Interfaces;

public interface IUserRepository
{
    Task<long> AddUserAsync(User user);
    Task<User> GetUserByGoogleId(string googleId);
    Task<User> GetUserByIdAsync(long id);
    Task UpdateUserAsync(User user);
    Task<User> GetUserByEmailAsync(string email);
    Task<User?> GetWithOrdersAndCardByTelegramIdAsync(long telegramId);
    //Task<User> GetUserByUserNameAsync(string userName);
    Task UpdateUserRoleAsync(long userId, string userRole);
    Task DeleteUserByIdAsync(long userId);
}
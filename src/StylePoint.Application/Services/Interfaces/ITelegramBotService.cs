using StylePoint.Domain.Entities;

namespace StylePoint.Application.Services.Interfaces;

public interface ITelegramBotService
{
    Task NotifyNewProductAsync(Product product);
}

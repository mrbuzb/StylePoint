using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Domain.Entities;
using StylePoint.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace StylePoint.Infrastructure.Persistence.TgService;

public class OrderService
{
    private readonly ITelegramBotClient _botClient;
    private readonly AppDbContext _context;
    private const int PageSize = 3;

    public OrderService(ITelegramBotClient botClient, AppDbContext context)
    {
        _botClient = botClient;
        _context = context;
    }

    public async Task ShowAddressesAsync(long chatId, int page = 1)
    {
        var user = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.TelegramId == chatId);

        if (user == null)
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Siz ro'yxatdan o'tmagansiz.");
            return;
        }

        var totalAddresses = user.Addresses.Count;
        if (totalAddresses == 0)
        {
            await _botClient.SendTextMessageAsync(chatId, "📦 Sizda manzil mavjud emas.");
            return;
        }

        var totalPages = (int)Math.Ceiling(totalAddresses / (decimal)PageSize);
        page = Math.Max(1, Math.Min(page, totalPages));

        var addresses = user.Addresses
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        var buttons = new List<InlineKeyboardButton[]>();
        foreach (var addr in addresses)
        {
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{addr.City}, {addr.Address}",
                    $"selectAddress_{addr.Id}")
            });
        }

        var navRow = new List<InlineKeyboardButton>();
        if (page > 1)
            navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️ Oldingi", $"pageAddr_{page - 1}"));
        if (page < totalPages)
            navRow.Add(InlineKeyboardButton.WithCallbackData("➡️ Keyingi", $"pageAddr_{page + 1}"));

        if (navRow.Any()) buttons.Add(navRow.ToArray());

        await _botClient.SendTextMessageAsync(chatId, "📍 Manzilingizni tanlang:",
            replyMarkup: new InlineKeyboardMarkup(buttons));
    }

    public async Task CreateOrderAsync(long chatId, long addressId)
    {
        var user = await _context.Users
            .Include(u => u.CartItems)
                .ThenInclude(ci => ci.ProductVariant)
                    .ThenInclude(v => v.Product)
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.TelegramId == chatId);

        if (user == null)
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Siz ro'yxatdan o'tmagansiz.");
            return;
        }

        var address = user.Addresses.FirstOrDefault(a => a.Id == addressId);
        if (address == null)
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Manzil topilmadi.");
            return;
        }

        if (!user.CartItems.Any())
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Savatingiz bo‘sh.");
            return;
        }

        var order = new Order
        {
            UserId = user.UserId,
            AddressId = address.Id,
            Status = OrderStatus.Pending,
            TotalPrice = user.CartItems.Sum(ci => ci.Quantity * ci.UnitPrice)
        };

        foreach (var ci in user.CartItems)
        {
            order.OrderItems.Add(new OrderItem
            {
                ProductVariantId = ci.ProductVariantId,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice
            });
        }

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(user.CartItems);
        await _context.SaveChangesAsync();

        await _botClient.SendTextMessageAsync(chatId, $"✅ Buyurtma yaratildi! Umumiy summa: {order.TotalPrice} $");
    }

    public async Task HandleCallbackQueryAsync(Telegram.Bot.Types.CallbackQuery query)
    {
        if (query.Data == null) return;

        if (query.Data.StartsWith("pageAddr_"))
        {
            int page = int.Parse(query.Data.Replace("pageAddr_", ""));
            await ShowAddressesAsync(query.Message.Chat.Id, page);
        }
        else if (query.Data.StartsWith("selectAddress_"))
        {
            long addressId = long.Parse(query.Data.Replace("selectAddress_", ""));
            await CreateOrderAsync(query.Message.Chat.Id, addressId);
        }
    }
}

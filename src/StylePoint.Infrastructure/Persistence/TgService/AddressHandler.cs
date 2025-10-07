using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Application.Dtos;
using StylePoint.Domain.Entities;
using System.Collections.Concurrent;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.DependencyInjection;

namespace StylePoint.Infrastructure.Persistence.TgService;

public class AddressHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<long, AddressSession> _sessions = new();

    public AddressHandler(ITelegramBotClient botClient, IServiceScopeFactory scopeFactory)
    {
        _botClient = botClient;
        _scopeFactory = scopeFactory;
    }

    private AppDbContext CreateContext()
    {
        var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    // Menyu ko‘rsatish
    public async Task ShowAddressMenuAsync(long chatId)
    {
        var buttons = new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("📋 MyAddresses", "myAddresses") },
            new[] { InlineKeyboardButton.WithCallbackData("➕ CreateAddress", "createAddress") }
        };

        await _botClient.SendTextMessageAsync(
            chatId,
            "Adreslar bo‘limi. Iltimos tanlang:",
            replyMarkup: new InlineKeyboardMarkup(buttons)
        );
    }

    // CallbackQuery handle
    public async Task HandleCallbackQueryAsync(CallbackQuery query)
    {
        if (query.Data == null) return;

        if (query.Data.StartsWith("myAddresses"))
        {
            await ShowUserAddressesAsync(query.Message.Chat.Id, 1);
        }
        else if (query.Data.StartsWith("pageAddr_"))
        {
            int page = int.Parse(query.Data.Replace("pageAddr_", ""));
            await ShowUserAddressesAsync(query.Message.Chat.Id, page, query.Message.MessageId);
        }
        else if (query.Data.StartsWith("createAddress"))
        {
            _sessions[query.Message.Chat.Id] = new AddressSession();
            await _botClient.SendTextMessageAsync(
                query.Message.Chat.Id,
                "Yangi adres yaratish. Iltimos, to‘liq ismni kiriting:"
            );
        }
    }

    // User xabarlarini ketma-ket qabul qilish
    public async Task HandleUserMessageAsync(Message message)
    {
        var chatId = message.Chat.Id;
        var text = message.Text ?? "";

        if (!_sessions.ContainsKey(chatId))
            return;

        var session = _sessions[chatId];

        if (string.IsNullOrEmpty(session.FullName))
        {
            session.FullName = text;
            await _botClient.SendTextMessageAsync(chatId, "Iltimos, adresni kiriting:");
        }
        else if (string.IsNullOrEmpty(session.Address))
        {
            session.Address = text;
            await _botClient.SendTextMessageAsync(chatId, "Shahar nomini kiriting:");
        }
        else if (string.IsNullOrEmpty(session.City))
        {
            session.City = text;
            await _botClient.SendTextMessageAsync(chatId, "Postal kodni kiriting:");
        }
        else if (string.IsNullOrEmpty(session.PostalCode))
        {
            session.PostalCode = text;
            await _botClient.SendTextMessageAsync(chatId, "Mamlakat nomini kiriting:");
        }
        else if (string.IsNullOrEmpty(session.Country))
        {
            session.Country = text;

            using var context = CreateContext(); // Har safar yangi context
            var user = await context.Users.FirstOrDefaultAsync(u => u.TelegramId == chatId);
            if (user != null)
            {
                var addr = new DeliveryAddress
                {
                    UserId = user.UserId,
                    FullName = session.FullName,
                    Address = session.Address,
                    City = session.City,
                    PostalCode = session.PostalCode,
                    Country = session.Country
                };

                await context.DeliveryAddresses.AddAsync(addr);
                await context.SaveChangesAsync();

                await _botClient.SendTextMessageAsync(chatId, "✅ Adres muvaffaqiyatli saqlandi!");
            }

            _sessions.TryRemove(chatId, out _);
        }
    }

    // Foydalanuvchi adreslarini ko‘rsatish
    private async Task ShowUserAddressesAsync(long chatId, int page = 1, int? messageId = null)
    {
        using var context = CreateContext(); // Har safar yangi context
        const int pageSize = 1;

        var user = await context.Users.FirstOrDefaultAsync(u => u.TelegramId == chatId);
        if (user == null)
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Siz topilmadingiz.");
            return;
        }

        var addresses = await context.DeliveryAddresses
            .Where(a => a.UserId == user.UserId)
            .OrderBy(a => a.Id)
            .ToListAsync();

        if (!addresses.Any())
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Sizda adreslar mavjud emas.");
            return;
        }

        var totalPages = (int)Math.Ceiling(addresses.Count / (double)pageSize);
        page = Math.Max(1, Math.Min(page, totalPages));

        var addr = addresses.Skip(page - 1).Take(pageSize).First();

        var textMessage = new StringBuilder();
        textMessage.AppendLine($"👤 <b>{addr.FullName}</b>");
        textMessage.AppendLine($"🏠 {addr.Address}");
        textMessage.AppendLine($"🌆 {addr.City}");
        textMessage.AppendLine($"📮 {addr.PostalCode}");
        textMessage.AppendLine($"🌍 {addr.Country}");

        var navRow = new List<InlineKeyboardButton>();
        if (page > 1)
            navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️ Oldingi", $"pageAddr_{page - 1}"));
        if (page < totalPages)
            navRow.Add(InlineKeyboardButton.WithCallbackData("➡️ Keyingi", $"pageAddr_{page + 1}"));

        var buttons = navRow.Any() ? new InlineKeyboardMarkup(navRow) : null;

        if (messageId.HasValue)
        {
            await _botClient.EditMessageTextAsync(chatId, messageId.Value, textMessage.ToString(),
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: buttons);
        }
        else
        {
            await _botClient.SendTextMessageAsync(chatId, textMessage.ToString(),
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: buttons);
        }
    }

    private class AddressSession : DeliveryAddressCreateDto { }
}

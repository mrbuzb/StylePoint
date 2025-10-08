using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StylePoint.Application.Dtos;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Implementations;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Domain.Entities;
using StylePoint.Domain.Enums;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace StylePoint.Infrastructure.Persistence.TgService;

public class TgBotService : BackgroundService
{
    private readonly ILogger<TgBotService> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AddressHandler _addressHandler;
    private readonly ProductBotService _productBotService;
    public TgBotService(ILogger<TgBotService> logger, IConfiguration config, IServiceScopeFactory scopeFactory, ProductBotService productBotService)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        string token = config["TelegramBot:Token"]
            ?? throw new InvalidOperationException("Telegram bot token not found in configuration.");
        _botClient = new TelegramBotClient(token);

        _addressHandler = new AddressHandler(_botClient, _scopeFactory);
        _productBotService =new ProductBotService(_botClient,_scopeFactory);
    }



    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🤖 Telegram bot started...");

        int offset = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates = await _botClient.GetUpdatesAsync(offset, cancellationToken: stoppingToken);

                foreach (var update in updates)
                {
                    offset = update.Id + 1;
                    await HandleUpdateAsync(update);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telegram bot error occurred.");
                await Task.Delay(2000, stoppingToken);
            }
        }

        _logger.LogInformation("🛑 Telegram bot stopped.");
    }

    private async Task HandleUpdateAsync(Update update)
    {
        if (update.Type == UpdateType.CallbackQuery)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var paginationHandler = new ProductPaginationHandler(_botClient, context);
            var query = update.CallbackQuery!;

            // ProductPaginationHandler callbacklari
            if (query.Data!.StartsWith("page_")
                || query.Data.StartsWith("variant_")
                || query.Data.StartsWith("addcart_")
                || query.Data.StartsWith("addcartvariant_"))
            {
                await paginationHandler.HandleCallbackQueryAsync(query);
            }
            // ProductBotService callbacklari (filter, filteritem)
            else if (query.Data.StartsWith("filter_") || query.Data.StartsWith("filteritem_"))
            {
                await _productBotService.HandleCallbackQueryAsync(query);
            }
            // Address tugmalari
            else if (query.Data.StartsWith("myAddresses")
                     || query.Data.StartsWith("pageAddr_")
                     || query.Data.StartsWith("createAddress"))
            {
                await _addressHandler.HandleCallbackQueryAsync(query);
            }
            else if (query.Data.StartsWith("pageAddr_") || query.Data.StartsWith("selectAddress_"))
            {
                var orderService = new OrderService(_botClient, context);
                await orderService.HandleCallbackQueryAsync(query);
            }
            // Savatdan o‘chirish
            if (query.Data.StartsWith("removeCartItem_"))
            {
                var idStr = query.Data.Split("_")[1];
                if (long.TryParse(idStr, out var cartItemId))
                {
                    var item = await context.CartItems
                        .Include(ci => ci.ProductVariant)
                        .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

                    if (item != null)
                    {
                        context.CartItems.Remove(item);
                        await context.SaveChangesAsync();

                        await _botClient.AnswerCallbackQueryAsync(query.Id, "✅ Mahsulot savatdan o‘chirildi!");
                        await _botClient.DeleteMessageAsync(query.Message.Chat.Id, query.Message.MessageId);
                    }
                    else
                    {
                        await _botClient.AnswerCallbackQueryAsync(query.Id, "❌ Mahsulot topilmadi.");
                    }
                }
            }
            // Bekor qilish
            if (query.Data == "cancel")
            {
                if (query.Message != null)
                {
                    await _botClient.EditMessageTextAsync(
                        chatId: query.Message.Chat.Id,
                        messageId: query.Message.MessageId,
                        text: "❌ Amal bekor qilindi."
                    );
                }
            }
            if(query.Data.StartsWith("categoryFilter") ||
                query.Data.StartsWith("tagFilter")||
                query.Data.StartsWith("brandFilter"))
            {

            }

            // To‘lov tugmalari
            if (query.Data.StartsWith("payOrderCard_") || query.Data.StartsWith("payOrderCash_"))
            {
                var idStr = query.Data.Split("_")[1];
                if (long.TryParse(idStr, out var paymentId))
                {
                    var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                    var dto = new PaymentCreateDto
                    {
                        OrderId = paymentId,
                        Method = query.Data.StartsWith("payOrderCard_") ? PaymentMethod.Card : PaymentMethod.Cash
                    };

                    try
                    {
                        var paymentDto = await paymentService.ProcessTelegramPaymentAsync(query.Message.Chat.Id, dto);
                        await _botClient.AnswerCallbackQueryAsync(query.Id, "✅ To‘lov amalga oshirildi!");
                        await _botClient.EditMessageTextAsync(
                            query.Message.Chat.Id,
                            query.Message.MessageId,
                            $"✅ Order {paymentDto.OrderId} uchun to‘lov muvaffaqiyatli amalga oshirildi."
                        );
                    }
                    catch (Exception ex)
                    {
                        await _botClient.AnswerCallbackQueryAsync(query.Id, $"❌ Xato: {ex.Message}");
                    }
                }
            }

            return;
        }


        if (update.Type != UpdateType.Message || update.Message == null)
            return;

        var message = update.Message;
        var chatId = message.Chat.Id;
        var text = message.Text ?? "";

        using var scope2 = _scopeFactory.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleRepo = scope2.ServiceProvider.GetRequiredService<IRoleRepository>();
        var paginationHandler2 = new ProductPaginationHandler(_botClient, context2);
        //var adressHandler2 = new AddressHandler(_botClient, context2);

        if (text.StartsWith("/start"))
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
    {
        new KeyboardButton[] { "📦 Mahsulotlar", "🛒 Savat", "📦 Buyurtmalar" },
        new KeyboardButton[] { "💳 To‘lovlar", "👛 Hamyon", "🏠 Manzillar" },
        new KeyboardButton[] { "💰 To‘lash", "🧾 Order yaratish", "❓ Yordam" },
        new KeyboardButton[] { "🔎 Mahsulotlarni Qidirish" }
    })
            {
                ResizeKeyboard = true,   // Tugmalar ekran o'lchamiga moslashadi
                OneTimeKeyboard = false  // Tugmalar doim ko‘rinadi
            };

            await _botClient.SendTextMessageAsync(
                chatId,
                "Salom 👋 StylePoint onlayn do‘kon botiga xush kelibsiz! Quyidagi tugmalardan foydalanishingiz mumkin:",
                replyMarkup: keyboard
            );


            var exists = await context2.Users.AnyAsync(x => x.TelegramId == chatId);
            if (!exists)
            {
                var user = new Domain.Entities.User
                {
                    TelegramId = chatId,
                    FirstName = message.Chat.FirstName ?? "Anon",
                    LastName = message.Chat.LastName ?? "Anon",
                    RoleId = await roleRepo.GetRoleIdAsync("User"),
                };

                await context2.Users.AddAsync(user);
                await context2.SaveChangesAsync();

                var card = new Card
                {
                    UserId = user.UserId,
                    Balance = 0,
                    CardNumber = Guid.NewGuid()
                };

                await context2.Cards.AddAsync(card);
                await context2.SaveChangesAsync();
            }
        }
        else if (text.Equals("❓ Yordam"))
        {
            await _botClient.SendTextMessageAsync(chatId,
                "Admin : @dotned");
        }
        else if (text.Equals("📦 Mahsulotlar"))
        {
            await paginationHandler2.ShowProductAsync(chatId, 1);
        }
        else if (text.Equals("📦 Buyurtmalar"))
        {
            // Foydalanuvchining buyurtmalarini olish

            var user = await context2.Users
                .Include(x=>x.Orders)
                .FirstOrDefaultAsync(o => o.TelegramId == chatId); // chatId yoki userId bo'lishi mumkin
            var orders = user.Orders.ToList();

            if (orders.Count == 0)
            {
                await _botClient.SendTextMessageAsync(chatId, "📦 Sizda hozircha buyurtmalar mavjud emas.");
            }
            else
            {
                var message2 = new StringBuilder("📦 Sizning buyurtmalaringiz:\n\n");
                foreach (var order in orders)
                {
                    if(order.Status != OrderStatus.Completed)
                    {
                        message2.AppendLine($"ID: {order.Id}");
                        message2.AppendLine($"Status: {order.Status}");
                        message2.AppendLine($"Umumiy summa: {order.TotalPrice:C}");
                        message2.AppendLine($"Sana: {order.CreatedAt:dd.MM.yyyy}");
                        message2.AppendLine("---------------------------");
                    }
                }

                await _botClient.SendTextMessageAsync(chatId, message2.ToString());
            }
        }
        else if (text.Equals("💳 To‘lovlar"))
        {
            var user =await context2.Users.FirstOrDefaultAsync(x=>x.TelegramId == chatId);
            // Foydalanuvchining to'lovlarini olish
            var payments = await context2.Payments
                .Include(x => x.Order)
                .Where(p => p.Order.UserId == user.UserId).ToListAsync();

            if (payments.Count == 0)
            {
                await _botClient.SendTextMessageAsync(chatId, "💳 Sizda hozircha to‘lovlar mavjud emas.");
            }
            else
            {
                var message2 = new StringBuilder("💳 Sizning to‘lovlaringiz:\n\n");
                foreach (var payment in payments)
                {
                    message2.AppendLine($"ID: {payment.Id}");
                    message2.AppendLine($"Summasi: {payment.Amount:C}");
                    message2.AppendLine($"Payment Status: {payment.Status}");
                    message2.AppendLine($"Payment Method: {payment.Method}");
                    message2.AppendLine($"Sana: {payment.PaidAt:dd.MM.yyyy}");
                    message2.AppendLine("---------------------------");
                }

                await _botClient.SendTextMessageAsync(chatId, message2.ToString());
            }
        }

        else if (text.Equals("🏠 Manzillar"))
        {
            await _addressHandler.ShowAddressMenuAsync(chatId);
        }

        else if (text.Equals("🔎 Mahsulotlarni Qidirish"))
        {
            await _productBotService.HandleSearchCommandAsync(chatId);
        }
        else if (text.Equals("💰 To‘lash"))
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

            // Foydalanuvchini topamiz
            var user = await context.Users
                .Include(u => u.Orders)
                .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(u => u.TelegramId == chatId);

            if (user == null)
            {
                await _botClient.SendTextMessageAsync(chatId, "❌ Foydalanuvchi topilmadi.");
                return;
            }

            // Pending statusdagi orderlarni olish
            var pendingOrders = user.Orders
                .Where(o => o.Status == OrderStatus.Pending)
                .ToList();

            if (!pendingOrders.Any())
            {
                await _botClient.SendTextMessageAsync(chatId, "✅ Sizda to‘lov qilinishi kerak bo‘lgan orderlar mavjud emas.");
                return;
            }

            foreach (var order in pendingOrders)
            {
                var payment = await context.Payments.FirstOrDefaultAsync(p => p.OrderId == order.Id);
                if (payment == null)
                {
                    payment = new Payment
                    {
                        Amount = order.TotalPrice,
                        Status = PaymentStatus.Pending,
                        OrderId = order.Id
                    };
                    await context.Payments.AddAsync(payment);
                    await context.SaveChangesAsync();
                }

                var keyboard = new InlineKeyboardMarkup(new[]
                {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "💳 To‘lash (Kartadan)",
                    $"payOrderCard_{order.Id}"
                ),
                InlineKeyboardButton.WithCallbackData(
                    "💵 Naqd to‘lash",
                    $"payOrderCash_{order.Id}"
                )
            }
        });

                var messageText = new StringBuilder();
                messageText.AppendLine($"🧾 Order ID: {order.Id}");
                messageText.AppendLine($"📦 Mahsulotlar soni: {order.OrderItems.Count}");
                messageText.AppendLine($"💰 To‘lov miqdori: {payment.Amount} $"); 
                messageText.AppendLine($"⏳ Status: {payment.Status}");

                await _botClient.SendTextMessageAsync(chatId, messageText.ToString(), replyMarkup: keyboard);
            }
        }




        else if (text.Equals("\U0001f6d2 Savat"))
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Foydalanuvchini topamiz
            var user = await context.Users
                .Include(u => u.Card)
                .Include(c => c.CartItems)
                .ThenInclude(x=>x.ProductVariant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(u => u.TelegramId == chatId);

            if (user == null || user.Card == null || !user.CartItems.Any())
            {
                await _botClient.SendTextMessageAsync(chatId, "🛒 Savatingiz bo‘sh.");
                return;
            }

            foreach (var item in user.CartItems)
            {
                var variant = item.ProductVariant;

                var keyboard = new InlineKeyboardMarkup(new[]
                {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "🗑️ Olib tashlash",
                    $"removeCartItem_{item.Id}"
                )
            }
        });

                var messageText = new StringBuilder();
                messageText.AppendLine($"📦 Mahsulot: {variant.Product.Name}");
                messageText.AppendLine($"🔹 Variant: {variant.Color}");
                messageText.AppendLine($"💰 Umumiy Narxi: {variant.Price*item.Quantity} $");
                messageText.AppendLine($"🔢 Miqdor: {item.Quantity}");

                await _botClient.SendTextMessageAsync(
                    chatId,
                    messageText.ToString(),
                    replyMarkup: keyboard
                );
            }
        }


        else if (text.Equals("👛 Hamyon"))
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Foydalanuvchini topamiz
            var user = await context.Users
                .Include(u => u.Card) // Card relation mavjud bo‘lsa
                .FirstOrDefaultAsync(u => u.TelegramId == chatId);

            if (user == null || user.Card == null)
            {
                await _botClient.SendTextMessageAsync(chatId, "❌ Sizning kartangiz topilmadi.");
                return;
            }

            var card = user.Card;

            var textMessage = new StringBuilder();
            textMessage.AppendLine($"💳 Kartangiz: <b>{card.CardNumber}</b>");
            textMessage.AppendLine($"💰 Balans: <b>{card.Balance} $</b>");
            textMessage.AppendLine();
            textMessage.AppendLine("⚠️ Balansni to‘ldirish uchun admin bilan bog‘laning: @dotned");

            await _botClient.SendTextMessageAsync(
                chatId,
                textMessage.ToString(),
                parseMode: ParseMode.Html
            );
        }

        else if (text.Equals("\U0001f9fe Order yaratish"))
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var orderService = new OrderService(_botClient, context);

            await orderService.ShowAddressesAsync(chatId, 1);
        }


        else
        {
            await _addressHandler.HandleUserMessageAsync(message);
        }
    }
}

using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Domain.Entities;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace StylePoint.Infrastructure.Persistence.TgService;

public class ProductPaginationHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly AppDbContext _context;

    public ProductPaginationHandler(ITelegramBotClient botClient, AppDbContext context)
    {
        _botClient = botClient;
        _context = context;
    }
    private async Task HandleAddVariantToCartAsync(long chatId, int variantId)
    {
        // Telegram chatId orqali userni topamiz
        var user = await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == chatId);
        if (user == null)
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Siz ro'yxatdan o'tmagansiz.");
            return;
        }

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == variantId);

        if (variant == null)
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Variant topilmadi.");
            return;
        }

        // Agar userda allaqachon shu variant bo'lsa, quantityni oshiramiz
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == user.UserId && ci.ProductVariantId == variantId);

        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            var cartItem = new CartItem
            {
                UserId = user.UserId,
                ProductVariantId = variant.Id,
                Quantity = 1,
                UnitPrice = variant.Price
            };
            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();

        await _botClient.SendTextMessageAsync(
            chatId,
            $"✅ <b>{variant.Product.Name} ({variant.Color} | {variant.Size})</b> savatga qo‘shildi!",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
        );
    }


    public async Task ShowProductAsync(long chatId, int page = 1, int? messageId = null)
    {
        var totalCount = await _context.Products.CountAsync();
        if (totalCount == 0)
        {
            await _botClient.SendTextMessageAsync(chatId, "📦 Hozircha mahsulotlar mavjud emas.");
            return;
        }

        var totalPages = (int)Math.Ceiling(totalCount / 1.0); // har safar 1 ta product
        page = Math.Max(1, Math.Min(page, totalPages));

        var product = await _context.Products
            .Include(p => p.Variants)
            .OrderBy(p => p.Id)
            .Skip(page - 1)
            .Take(1)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Mahsulot topilmadi.");
            return;
        }

        var captionBuilder = new StringBuilder();
        captionBuilder.AppendLine($"<b>{product.Name}</b>");
        captionBuilder.AppendLine(product.DiscountPrice.HasValue
            ? $"💰 <b>{product.DiscountPrice} $</b> (avval {product.Price} $)"
            : $"💰 <b>{product.Price} $</b>");
        if (!string.IsNullOrEmpty(product.Description))
            captionBuilder.AppendLine($"\n{product.Description}");

        // === Variant tugmalari ===
        var buttons = new List<InlineKeyboardButton[]>();

        if (product.Variants != null && product.Variants.Any())
        {
            foreach (var v in product.Variants)
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{v.Color} | {v.Size} ({v.Price} $)", $"variant_{v.Id}")
                });
            }
        }
        else
        {
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("🛒 Savatga qo‘shish", $"addcart_{product.Id}")
            });
        }

        // Navigatsiya tugmalari
        var navRow = new List<InlineKeyboardButton>();
        if (page > 1)
            navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️ Oldingi", $"page_{page - 1}"));
        if (page < totalPages)
            navRow.Add(InlineKeyboardButton.WithCallbackData("➡️ Keyingi", $"page_{page + 1}"));
        if (navRow.Any()) buttons.Add(navRow.ToArray());

        var replyMarkup = new InlineKeyboardMarkup(buttons);

        // Eski xabarni yangilaymiz yoki yangisini yuboramiz
        try
        {
            if (messageId.HasValue)
            {
                await _botClient.EditMessageMediaAsync(
                    chatId: chatId,
                    messageId: messageId.Value,
                    media: new InputMediaPhoto(
                        product.ImageUrl.StartsWith("data:image")
                            ? InputFile.FromStream(new MemoryStream(Convert.FromBase64String(product.ImageUrl.Split(',')[1])))
                            : InputFile.FromUri(product.ImageUrl)
                    )
                );

                await _botClient.EditMessageCaptionAsync(
                    chatId: chatId,
                    messageId: messageId.Value,
                    caption: captionBuilder.ToString(),
                    parseMode: ParseMode.Html,
                    replyMarkup: replyMarkup
                );
            }
            else
            {
                if (product.ImageUrl.StartsWith("data:image"))
                {
                    var base64Data = product.ImageUrl.Substring(product.ImageUrl.IndexOf(",") + 1);
                    var bytes = Convert.FromBase64String(base64Data);
                    await using var stream = new MemoryStream(bytes);
                    await _botClient.SendPhotoAsync(
                        chatId,
                        photo: InputFile.FromStream(stream, "product.jpg"),
                        caption: captionBuilder.ToString(),
                        parseMode: ParseMode.Html,
                        replyMarkup: replyMarkup
                    );
                }
                else
                {
                    await _botClient.SendPhotoAsync(
                        chatId,
                        photo: InputFile.FromUri(product.ImageUrl),
                        caption: captionBuilder.ToString(),
                        parseMode: ParseMode.Html,
                        replyMarkup: replyMarkup
                    );
                }
            }
        }
        catch (Exception ex)
        {
            await _botClient.SendTextMessageAsync(chatId, $"⚠️ Xato: {ex.Message}");
        }
    }

    public async Task HandleCallbackQueryAsync(CallbackQuery query)
    {
        if (query.Data == null)
            return;

        if (query.Data.StartsWith("page_"))
        {
            int page = int.Parse(query.Data.Replace("page_", ""));
            await ShowProductAsync(query.Message.Chat.Id, page, query.Message.MessageId);
        }
        else if (query.Data.StartsWith("variant_"))
        {
            int variantId = int.Parse(query.Data.Replace("variant_", ""));
            await HandleVariantSelectionAsync(query.Message.Chat.Id, variantId);
        }
        else if (query.Data.StartsWith("addcartvariant_"))
        {
            int variantId = int.Parse(query.Data.Replace("addcartvariant_", ""));
            await HandleAddVariantToCartAsync(query.Message.Chat.Id, variantId);
        }
        else if (query.Data.StartsWith("addcart_"))
        {
            int variantId = int.Parse(query.Data.Replace("addcart_", ""));
            await HandleAddVariantToCartAsync(query.Message.Chat.Id, variantId);
        }
        

    }

    private async Task HandleVariantSelectionAsync(long chatId, int variantId)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == variantId);

        if (variant == null)
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Variant topilmadi.");
            return;
        }

        var confirmButtons = new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData("✅ Tasdiqlash", $"addcartvariant_{variant.Id}"),
                InlineKeyboardButton.WithCallbackData("❌ Bekor qilish", "cancel")
            }
        };

        var text = $"🛍 <b>{variant.Product.Name}</b>\n" +
                   $"🔳 Rang: {variant.Color}\n" +
                   $"📏 O‘lcham: {variant.Size}\n" +
                   $"💰 Narx: {variant.Price} $\n\n" +
                   $"🧮 Zaxira: {variant.Stock}\n\n" +
                   $"Savatga qo‘shmoqchimisiz?";

        await _botClient.SendTextMessageAsync(
            chatId,
            text,
            parseMode: ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup(confirmButtons)
        );
    }

}

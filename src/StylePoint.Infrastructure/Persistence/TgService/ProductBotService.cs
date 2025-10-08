using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StylePoint.Domain.Entities;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace StylePoint.Infrastructure.Persistence.TgService;

public class ProductBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceScopeFactory _scopeFactory;

    // sessionlar
    private readonly Dictionary<long, string> _userFilterSession = new();
    private readonly Dictionary<long, List<Product>> _userProductsSession = new();

    public ProductBotService(
        ITelegramBotClient botClient, IServiceScopeFactory context)
    {
        _botClient = botClient;
        _scopeFactory = context;
    }

    private AppDbContext CreateContext()
    {
        var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    // 🔎 Mahsulot qidirish tugmasi bosilganda
    public async Task HandleSearchCommandAsync(long chatId)
    {
        var buttons = new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("📂 Category", "filter_category") },
            new[] { InlineKeyboardButton.WithCallbackData("🏷️ Brand", "filter_brand") },
            new[] { InlineKeyboardButton.WithCallbackData("🏷️ Tag", "filter_tag") },
            new[] { InlineKeyboardButton.WithCallbackData("🔥 Bestsellers", "filter_bestsellers") },
            new[] { InlineKeyboardButton.WithCallbackData("🆕 New arrivals", "filter_new") },
        };

        await _botClient.SendTextMessageAsync(chatId, "🔎 Mahsulotlarni qidirish usulini tanlang:",
            replyMarkup: new InlineKeyboardMarkup(buttons));
    }

    // Foydalanuvchi filter turini tanladi
    public async Task HandleFilterSelectionAsync(long chatId, string filterType)
    {
        _userFilterSession[chatId] = filterType;
        var _context = CreateContext();

        if (filterType == "bestsellers")
        {
            var products = (await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .OrderByDescending(p => p.Price)
            .Take(10)
            .ToListAsync());

            _userProductsSession[chatId] = products;
            await ShowProductAsync(chatId, products, 1);
            return;
        }
        else if (filterType == "new")
        {
            var products = (await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .OrderByDescending(p => p.Id)
            .Take(10)
            .ToListAsync());
            _userProductsSession[chatId] = products;
            await ShowProductAsync(chatId, products, 1);
            return;
        }

        IEnumerable<(long Id, string Name)> items = filterType switch
        {
            "category" => (await _context.Categories.ToListAsync()).Select(c => (c.Id, c.Name)),
            "brand" => (await _context.Brands.ToListAsync()).Select(b => (b.Id, b.Name)),
            "tag" => (await _context.Tags.ToListAsync()).Select(t => (t.Id, t.Name)),
            _ => Enumerable.Empty<(long, string)>()
        };

        var buttons = items.Select(i => new[] { InlineKeyboardButton.WithCallbackData(i.Name, $"filteritem_{i.Id}") }).ToList();

        await _botClient.SendTextMessageAsync(chatId,
            filterType switch
            {
                "category" => "📂 Kategoriya tanlang:",
                "brand" => "🏷️ Brend tanlang:",
                "tag" => "🏷️ Tag tanlang:",
                _ => "Tanlov"
            },
            replyMarkup: new InlineKeyboardMarkup(buttons));
    }

    // Foydalanuvchi filter item tanladi
    public async Task HandleFilterItemSelectionAsync(long chatId, long itemId)
    {
        if (!_userFilterSession.TryGetValue(chatId, out var filterType))
            return;
        var _context = CreateContext();
        List<Product> products = filterType switch
        {
            "category" => (await _context.Products.Where(x => x.CategoryId == itemId).ToListAsync()),
            "brand" => (await _context.Products.Where(x => x.BrandId == itemId).ToListAsync()),
            "tag" => (await _context.Products.Where(x => x.ProductTags.Any(x => x.TagId == itemId)).ToListAsync()),
            _ => new List<Product>()
        };

        if (!products.Any())
        {
            await _botClient.SendTextMessageAsync(chatId, "📦 Mahsulot topilmadi.");
            return;
        }

        _userProductsSession[chatId] = products;
        await ShowProductAsync(chatId, products, 1);
    }

    // Buyer dagi kabi product ko‘rsatish
    public async Task ShowProductAsync(long chatId, List<Product> products, int page = 1)
    {
        if (products == null || !products.Any())
        {
            await _botClient.SendTextMessageAsync(chatId, "📦 Mahsulot topilmadi.");
            return;
        }

        page = Math.Max(1, Math.Min(page, products.Count));
        var product = products[page - 1];

        var captionBuilder = new StringBuilder();
        captionBuilder.AppendLine($"<b>{product.Name}</b>");
        captionBuilder.AppendLine(product.DiscountPrice.HasValue
            ? $"💰 <b>{product.DiscountPrice} $</b> (avval {product.Price} $)"
            : $"💰 <b>{product.Price} $</b>");
        if (!string.IsNullOrEmpty(product.Description))
            captionBuilder.AppendLine($"\n{product.Description}");

        // Variant tugmalari
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

        // Navigatsiya
        var navRow = new List<InlineKeyboardButton>();
        if (page > 1)
            navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️ Oldingi", $"page_{page - 1}"));
        if (page < products.Count)
            navRow.Add(InlineKeyboardButton.WithCallbackData("➡️ Keyingi", $"page_{page + 1}"));
        if (navRow.Any()) buttons.Add(navRow.ToArray());

        var replyMarkup = new InlineKeyboardMarkup(buttons);

        if (!string.IsNullOrEmpty(product.ImageUrl))
        {
            if (product.ImageUrl.StartsWith("data:image"))
            {
                var base64Data = product.ImageUrl.Split(',')[1];
                var bytes = Convert.FromBase64String(base64Data);
                await using var stream = new MemoryStream(bytes);
                await _botClient.SendPhotoAsync(chatId,
                    InputFile.FromStream(stream, "product.jpg"),
                    caption: captionBuilder.ToString(),
                    parseMode: ParseMode.Html,
                    replyMarkup: replyMarkup);
            }
            else
            {
                await _botClient.SendPhotoAsync(chatId,
                    InputFile.FromUri(product.ImageUrl),
                    caption: captionBuilder.ToString(),
                    parseMode: ParseMode.Html,
                    replyMarkup: replyMarkup);
            }
        }
        else
        {
            await _botClient.SendTextMessageAsync(chatId,
                captionBuilder.ToString(),
                parseMode: ParseMode.Html,
                replyMarkup: replyMarkup);
        }
    }

    // Callback query handler
    public async Task HandleCallbackQueryAsync(CallbackQuery query)
    {
        if (query.Data == null) return;

        if (query.Data.StartsWith("filter_"))
        {
            var filterType = query.Data.Replace("filter_", "");
            await HandleFilterSelectionAsync(query.Message.Chat.Id, filterType);
        }
        else if (query.Data.StartsWith("filteritem_"))
        {
            var itemId = long.Parse(query.Data.Replace("filteritem_", ""));
            await HandleFilterItemSelectionAsync(query.Message.Chat.Id, itemId);
        }
        else if (query.Data.StartsWith("page_"))
        {
            int page = int.Parse(query.Data.Replace("page_", ""));
            if (_userProductsSession.TryGetValue(query.Message.Chat.Id, out var products))
            {
                await ShowProductAsync(query.Message.Chat.Id, products, page);
            }
        }
        else if (query.Data.StartsWith("variant_"))
        {
            int variantId = int.Parse(query.Data.Replace("variant_", ""));
            await HandleVariantSelectionAsync(query.Message.Chat.Id, variantId);
        }
    }

    private async Task HandleVariantSelectionAsync(long chatId, int variantId)
    {

        var _context = CreateContext();
        var variant = await _context.ProductVariants.FindAsync(variantId);
        if (variant == null)
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Variant topilmadi.");
            return;
        }

        var confirmButtons = new[]
        {
            new[]
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

        await _botClient.SendTextMessageAsync(chatId, text, parseMode: ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup(confirmButtons));
    }
}

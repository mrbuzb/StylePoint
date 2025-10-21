using StylePoint.Application.Dtos;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Core.Errors;
using StylePoint.Domain.Entities;

namespace StylePoint.Application.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly ICloudService _cloud;
    private readonly ITelegramBotService _telegramBotService;
    public ProductService(IProductRepository repo, ICloudService cloud, ITelegramBotService telegramBotService)
    {
        _repo = repo;
        _cloud = cloud;
        _telegramBotService = telegramBotService;
    }


    public async Task<bool> DeleteProductAsync(long productId)
    {
        var product = await _repo.GetByIdAsync(productId);
        if (product == null) return false;

        await _repo.DeleteAsync(product.Id);
        return true;
    }

    public async Task<long> AddProductAsync(ProductCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SecretCode))
        {
            throw new NotAllowedException("Secret Code Kiriting");
        }
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            ImageUrl = dto.Image,
            CategoryId = dto.CategoryId,
            BrandId = dto.BrandId,
            DiscountPrice = dto.DiscountPrice,
            Price = dto.Price,
            SecretCode = dto.SecretCode,
            Variants = dto.Variants.Select(v => new ProductVariant
            {
                Size = v.Size,
                Color = v.Color,
                Stock = v.Stock,
                Price = v.Price
            }).ToList(),
            ProductTags = dto.TagIds.Select(tagId => new ProductTag
            {
                TagId = tagId
            }).ToList()
        };


        var id =  await _repo.AddAsync(product);

        await _telegramBotService.NotifyNewProductAsync(product);
        return id;

    }


    public async Task<ProductDto> UpdateProductAsync(long productId, ProductUpdateDto dto)
    {
        var product = await _repo.GetByIdAsync(productId);
        if (product == null)
            throw new InvalidOperationException("Product not found.");

        product.Name = dto.Name ?? product.Name;
        product.Description = dto.Description ?? product.Description;
        product.Price = dto.Price ?? product.Price;
        product.DiscountPrice = dto.DiscountPrice ?? product.DiscountPrice;
        product.CategoryId = dto.CategoryId ?? product.CategoryId;
        product.BrandId = dto.BrandId ?? product.BrandId;
        product.SecretCode = dto.SecretCode ?? product.SecretCode;

        await _repo.UpdateAsync(product);
        return MapToDto(product);
    }


    public async Task<ICollection<ProductDto>> GetAllAsync()
    {
        var products = await _repo.GetAllAsync();
        return products.Select(MapToDto).ToList();
    }

    public async Task<ICollection<ProductDto>> GetBestSellersAsync()
    {
        var products = await _repo.GetBestSellersAsync();
        return products.Select(MapToDto).ToList();
    }

    public async Task<ICollection<ProductDto>> GetByBrandAsync(long brandId)
    {
        var products = await _repo.GetByBrandAsync(brandId);
        return products.Select(MapToDto).ToList();
    }

    public async Task<ICollection<ProductDto>> GetByCategoryAsync(long categoryId)
    {
        var products = await _repo.GetByCategoryAsync(categoryId);
        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(long id)
    {
        var product = await _repo.GetByIdAsync(id);
        return product is null ? null : MapToDto(product);
    }

    public async Task<ICollection<ProductDto>> GetByTagAsync(long tagId)
    {
        var products = await _repo.GetByTagAsync(tagId);
        return products.Select(MapToDto).ToList();
    }

    public async Task<ICollection<ProductDto>> GetNewArrivalsAsync()
    {
        var products = await _repo.GetNewArrivalsAsync();
        return products.Select(MapToDto).ToList();
    }

    public async Task<ICollection<ProductDto>> SearchAsync(string keyword)
    {
        var products = await _repo.SearchAsync(keyword);
        return products.Select(MapToDto).ToList();
    }

    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            ImageUrl = product.ImageUrl,
            CategoryName = product.Category.Name,
            BrandName = product.Brand.Name,
            SecretCode = product.SecretCode,
            Variants = product.Variants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                Size = v.Size,
                Color = v.Color,
                Stock = v.Stock,
                Price = v.Price
            }).ToList(),

            Tags = product.ProductTags.Select(t => t.Tag.Name).ToList()
        };
    }
}

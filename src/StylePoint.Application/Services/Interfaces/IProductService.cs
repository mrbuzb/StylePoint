using StylePoint.Application.Dtos;

namespace StylePoint.Application.Services.Interfaces;

public interface IProductService
{
    Task<ICollection<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(long id);
    Task<ICollection<ProductDto>> SearchAsync(string keyword);
    Task<ICollection<ProductDto>> GetByCategoryAsync(long categoryId);
    Task<ICollection<ProductDto>> GetByBrandAsync(long brandId);
    Task<ICollection<ProductDto>> GetByTagAsync(long tagId);
    Task<ICollection<ProductDto>> GetBestSellersAsync();
    Task<ICollection<ProductDto>> GetNewArrivalsAsync();
    Task<ProductDto> UpdateProductAsync(long productId, ProductUpdateDto dto);
    Task<ProductDto> AddProductAsync(ProductCreateDto dto);
    Task<bool> DeleteProductAsync(long productId);
}

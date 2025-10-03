using StylePoint.Application.Dtos;
using StylePoint.Application.Services.Interfaces;

namespace StylePoint.Application.Services.Implementations;

public class ProductService : IProductService
{
    public Task<ICollection<ProductDto>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<ProductDto>> GetBestSellersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<ProductDto>> GetByBrandAsync(long brandId)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<ProductDto>> GetByCategoryAsync(long categoryId)
    {
        throw new NotImplementedException();
    }

    public Task<ProductDto?> GetByIdAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<ProductDto>> GetByTagAsync(long tagId)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<ProductDto>> GetNewArrivalsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<ProductDto>> SearchAsync(string keyword)
    {
        throw new NotImplementedException();
    }
}

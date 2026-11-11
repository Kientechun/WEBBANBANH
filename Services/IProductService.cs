using BakeryShop.Models.Entities;
using BakeryShop.ViewModels;

namespace BakeryShop.Services;

public interface IProductService
{
    Task<HomeViewModel> GetHomeAsync(CancellationToken cancellationToken = default);
    Task<ProductListViewModel> SearchAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, string? sort, int page, CancellationToken cancellationToken = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

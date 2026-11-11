using BakeryShop.Data;
using BakeryShop.Models.Entities;
using BakeryShop.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Services;

public sealed class ProductService(BakeryDbContext db) : IProductService
{
    public async Task<HomeViewModel> GetHomeAsync(CancellationToken cancellationToken = default) => new()
    {
        Featured = await db.Products.AsNoTracking().Where(p => p.Status && p.IsFeatured).Take(8).ToListAsync(cancellationToken),
        NewProducts = await db.Products.AsNoTracking().Where(p => p.Status && p.IsNew).OrderByDescending(p => p.CreatedAt).Take(8).ToListAsync(cancellationToken),
        Bestsellers = await db.Products.AsNoTracking().Where(p => p.Status && p.IsBestseller).OrderByDescending(p => p.Views).Take(8).ToListAsync(cancellationToken),
        LatestNews = await db.News.AsNoTracking().Where(n => n.Status).OrderByDescending(n => n.CreatedAt).Take(3).ToListAsync(cancellationToken),
        Categories = await db.Categories.AsNoTracking().Where(c => c.Status).OrderBy(c => c.DisplayOrder).ToListAsync(cancellationToken)
    };

    public async Task<ProductListViewModel> SearchAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, string? sort, int page, CancellationToken cancellationToken = default)
    {
        const int pageSize = 12;
        var query = db.Products.AsNoTracking().Include(p => p.Category).Where(p => p.Status);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Name.Contains(search));
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
        if (minPrice.HasValue) query = query.Where(p => (p.SalePrice ?? p.Price) >= minPrice);
        if (maxPrice.HasValue) query = query.Where(p => (p.SalePrice ?? p.Price) <= maxPrice);
        query = sort switch
        {
            "price-asc" => query.OrderBy(p => p.SalePrice ?? p.Price),
            "price-desc" => query.OrderByDescending(p => p.SalePrice ?? p.Price),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };
        var total = await query.CountAsync(cancellationToken);
        page = Math.Max(1, page);
        return new ProductListViewModel
        {
            Products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken),
            Categories = await db.Categories.AsNoTracking().Where(c => c.Status).OrderBy(c => c.DisplayOrder).ToListAsync(cancellationToken),
            Search = search, CategoryId = categoryId, MinPrice = minPrice, MaxPrice = maxPrice, Sort = sort ?? "newest",
            Page = page, TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Slug == slug && p.Status, cancellationToken);
}

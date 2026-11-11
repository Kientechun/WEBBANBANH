using BakeryShop.Data;
using BakeryShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Controllers;

public class ProductsController(IProductService products, BakeryDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, string? sort, int page = 1, CancellationToken cancellationToken = default) =>
        View(await products.SearchAsync(search, categoryId, minPrice, maxPrice, sort, page, cancellationToken));

    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        var product = await products.GetBySlugAsync(slug, cancellationToken);
        if (product is null) return NotFound();
        product.Views++;
        await db.SaveChangesAsync(cancellationToken);
        ViewBag.Related = await db.Products.AsNoTracking().Where(p => p.Status && p.CategoryId == product.CategoryId && p.Id != product.Id).Take(4).ToListAsync(cancellationToken);
        return View(product);
    }
}

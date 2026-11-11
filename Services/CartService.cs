using System.Security.Claims;
using System.Text.Json;
using BakeryShop.Data;
using BakeryShop.Models.Entities;
using BakeryShop.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Services;

public sealed class CartService(BakeryDbContext db, IHttpContextAccessor accessor) : ICartService
{
    private const string SessionKey = "bakery_cart";
    private HttpContext Context => accessor.HttpContext ?? throw new InvalidOperationException("HTTP context is unavailable.");
    private int? UserId => int.TryParse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public async Task<CartViewModel> GetAsync(CancellationToken cancellationToken = default)
    {
        List<CartLineViewModel> lines;
        if (UserId is int userId)
            lines = await db.Cart.Where(c => c.UserId == userId).Include(c => c.Product)
                .Select(c => new CartLineViewModel { ProductId = c.ProductId, Name = c.Product.Name, Image = c.Product.Image, Price = c.Product.SalePrice ?? c.Product.Price, Quantity = c.Quantity, Stock = c.Product.Stock })
                .ToListAsync(cancellationToken);
        else
        {
            var stored = JsonSerializer.Deserialize<Dictionary<int, int>>(Context.Session.GetString(SessionKey) ?? "{}") ?? [];
            var products = await db.Products.Where(p => stored.Keys.Contains(p.Id) && p.Status).ToDictionaryAsync(p => p.Id, cancellationToken);
            lines = stored.Where(x => products.ContainsKey(x.Key)).Select(x =>
            {
                var p = products[x.Key];
                return new CartLineViewModel { ProductId = p.Id, Name = p.Name, Image = p.Image, Price = p.SalePrice ?? p.Price, Quantity = Math.Min(x.Value, p.Stock), Stock = p.Stock };
            }).Where(x => x.Quantity > 0).ToList();
        }
        return new CartViewModel { Items = lines };
    }

    public async Task<CartViewModel> SetQuantityAsync(int productId, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.Status, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");
        if (quantity < 0 || quantity > product.Stock) throw new InvalidOperationException("Số lượng vượt quá tồn kho.");
        if (UserId is int userId)
        {
            var row = await db.Cart.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId, cancellationToken);
            if (quantity == 0) { if (row is not null) db.Cart.Remove(row); }
            else if (row is null) db.Cart.Add(new CartItem { UserId = userId, ProductId = productId, Quantity = quantity });
            else row.Quantity = quantity;
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var items = JsonSerializer.Deserialize<Dictionary<int, int>>(Context.Session.GetString(SessionKey) ?? "{}") ?? [];
            if (quantity == 0) items.Remove(productId); else items[productId] = quantity;
            Context.Session.SetString(SessionKey, JsonSerializer.Serialize(items));
        }
        return await GetAsync(cancellationToken);
    }

    public Task<CartViewModel> RemoveAsync(int productId, CancellationToken cancellationToken = default) => SetQuantityAsync(productId, 0, cancellationToken);

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (UserId is int userId)
        {
            db.Cart.RemoveRange(db.Cart.Where(c => c.UserId == userId));
            await db.SaveChangesAsync(cancellationToken);
        }
        else Context.Session.Remove(SessionKey);
    }

    public async Task MergeSessionCartAsync(int userId, CancellationToken cancellationToken = default)
    {
        var items = JsonSerializer.Deserialize<Dictionary<int, int>>(Context.Session.GetString(SessionKey) ?? "{}") ?? [];
        foreach (var (productId, quantity) in items)
        {
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.Status, cancellationToken);
            if (product is null || product.Stock <= 0) continue;
            var row = await db.Cart.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId, cancellationToken);
            if (row is null) db.Cart.Add(new CartItem { UserId = userId, ProductId = productId, Quantity = Math.Min(quantity, product.Stock) });
            else row.Quantity = Math.Min(row.Quantity + quantity, product.Stock);
        }
        await db.SaveChangesAsync(cancellationToken);
        Context.Session.Remove(SessionKey);
    }
}

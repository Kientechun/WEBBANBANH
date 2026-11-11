using System.Security.Claims;
using System.Data;
using BakeryShop.Data;
using BakeryShop.Models.Entities;
using BakeryShop.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Services;

public sealed class OrderService(BakeryDbContext db, ICartService cartService, IHttpContextAccessor accessor) : IOrderService
{
    public async Task<Order> CheckoutAsync(CheckoutViewModel checkout, CancellationToken cancellationToken = default)
    {
        if (checkout.PaymentMethod is not ("cod" or "vnpay")) throw new InvalidOperationException("Phương thức thanh toán không hợp lệ.");
        var cart = await cartService.GetAsync(cancellationToken);
        if (cart.Items.Count == 0) throw new InvalidOperationException("Giỏ hàng đang trống.");
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var order = new Order
        {
            UserId = int.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null,
            OrderNumber = $"BK{DateTime.Now:yyyyMMdd}{Random.Shared.Next(100000, 999999)}",
            CustomerName = checkout.CustomerName, CustomerEmail = checkout.CustomerEmail,
            CustomerPhone = checkout.CustomerPhone, CustomerAddress = checkout.CustomerAddress,
            PaymentMethod = checkout.PaymentMethod, Notes = checkout.Notes, Status = "pending",
            PaymentStatus = "unpaid"
        };
        foreach (var line in cart.Items)
        {
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == line.ProductId && p.Status, cancellationToken)
                ?? throw new InvalidOperationException($"Sản phẩm {line.Name} hiện không còn kinh doanh.");
            if (line.Quantity <= 0 || product.Stock < line.Quantity)
                throw new InvalidOperationException($"Sản phẩm {product.Name} không đủ tồn kho.");
            var price = product.SalePrice ?? product.Price;
            product.Stock -= line.Quantity;
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id, ProductName = product.Name, ProductImage = product.Image,
                Price = price, Quantity = line.Quantity, Subtotal = price * line.Quantity
            });
        }
        order.TotalAmount = order.Items.Sum(i => i.Subtotal);
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        await cartService.ClearAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return order;
    }
}

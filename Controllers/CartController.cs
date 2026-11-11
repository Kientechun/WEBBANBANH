using BakeryShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace BakeryShop.Controllers;

public class CartController(ICartService cart) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await cart.GetAsync(cancellationToken));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity, CancellationToken cancellationToken)
    {
        try
        {
            var basket = await cart.GetAsync(cancellationToken);
            var current = basket.Items.FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0;
            var result = await cart.SetQuantityAsync(productId, current + Math.Max(1, quantity), cancellationToken);
            return Json(new { success = true, count = result.Count, total = result.Total, message = "Đã thêm vào giỏ hàng." });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetQuantity(int productId, int quantity, CancellationToken cancellationToken)
    {
        try
        {
            var result = await cart.SetQuantityAsync(productId, quantity, cancellationToken);
            return Json(new { success = true, count = result.Count, total = result.Total, message = "Đã cập nhật giỏ hàng." });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

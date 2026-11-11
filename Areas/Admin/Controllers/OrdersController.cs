using BakeryShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Policy = "AdminOnly")]
public class OrdersController(BakeryDbContext db, ILogger<OrdersController> logger) : Controller
{
    public async Task<IActionResult> Index(string? status, string? search, int page = 1, CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;
        var query = db.Orders.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(o => o.Status == status);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(o => o.OrderNumber.Contains(search) || o.CustomerName.Contains(search) || o.CustomerPhone.Contains(search));
        ViewBag.Status = status; ViewBag.Search = search; ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync(cancellationToken) / (double)pageSize);
        return View(await query.OrderByDescending(o => o.CreatedAt).Skip((Math.Max(1, page) - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string status, string paymentStatus, string? adminNotes, CancellationToken cancellationToken)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (order is null) return NotFound();
        var statuses = new[] { "pending", "confirmed", "processing", "shipping", "completed", "cancelled" };
        if (!statuses.Contains(status) || paymentStatus is not ("unpaid" or "paid"))
        {
            ModelState.AddModelError("", "Trạng thái cập nhật không hợp lệ.");
            return View("Detail", order);
        }
        if (order.Status == "cancelled" && status != "cancelled")
        {
            ModelState.AddModelError("", "Đơn đã hủy không thể mở lại.");
            return View("Detail", order);
        }
        if (status == "cancelled" && order.Status != "cancelled")
        {
            foreach (var item in order.Items)
                if (item.ProductId is int productId)
                {
                    var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
                    if (product is not null) product.Stock += item.Quantity;
                }
        }
        order.Status = status;
        order.PaymentStatus = paymentStatus;
        order.AdminNotes = adminNotes;
        order.UpdatedAt = DateTime.UtcNow;
        if (paymentStatus == "paid" && order.PaidAt is null) order.PaidAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Admin updated order {OrderNumber} to {Status}", order.OrderNumber, status);
        return RedirectToAction(nameof(Detail), new { id });
    }
}

using BakeryShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BakeryShop.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Policy = "AdminOnly")]
public class DashboardController(BakeryDbContext db) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.ProductCount = await db.Products.CountAsync(cancellationToken);
        ViewBag.OrderCount = await db.Orders.CountAsync(cancellationToken);
        ViewBag.CustomerCount = await db.Users.CountAsync(u => u.Role == "customer", cancellationToken);
        ViewBag.Revenue = await db.Orders.Where(o => o.PaymentStatus == "paid" || o.Status == "completed").SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0;
        ViewBag.Orders = await db.Orders.AsNoTracking().OrderByDescending(o => o.CreatedAt).Take(10).ToListAsync(cancellationToken);
        var year = DateTime.UtcNow.Year;
        var monthly = await db.Orders.AsNoTracking().Where(o => o.CreatedAt.Year == year && (o.PaymentStatus == "paid" || o.Status == "completed"))
            .GroupBy(o => o.CreatedAt.Month).Select(g => new { Month = g.Key, Total = g.Sum(o => o.TotalAmount) }).ToListAsync(cancellationToken);
        ViewBag.ChartLabels = JsonSerializer.Serialize(Enumerable.Range(1, 12).Select(m => $"{m:00}/{year}"));
        ViewBag.ChartValues = JsonSerializer.Serialize(Enumerable.Range(1, 12).Select(m => monthly.FirstOrDefault(x => x.Month == m)?.Total ?? 0));
        return View();
    }
}

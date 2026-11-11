using BakeryShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Policy = "AdminOnly")]
public class CustomersController(BakeryDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        var query = db.Users.AsNoTracking().Where(u => u.Role == "customer");
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search) || (u.Phone != null && u.Phone.Contains(search)));
        return View(await query.OrderByDescending(u => u.CreatedAt).ToListAsync(cancellationToken));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, CancellationToken cancellationToken)
    {
        var customer = await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "customer", cancellationToken);
        if (customer is null) return NotFound();
        customer.Status = !customer.Status;
        customer.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}

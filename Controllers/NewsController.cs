using BakeryShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Controllers;

public class NewsController(BakeryDbContext db) : Controller
{
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        const int pageSize = 9;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(await db.News.CountAsync(n => n.Status, cancellationToken) / (double)pageSize);
        return View(await db.News.AsNoTracking().Where(n => n.Status).OrderByDescending(n => n.CreatedAt).Skip((Math.Max(1, page) - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        var item = await db.News.FirstOrDefaultAsync(n => n.Slug == slug && n.Status, cancellationToken);
        if (item is null) return NotFound();
        item.Views++;
        await db.SaveChangesAsync(cancellationToken);
        return View(item);
    }
}

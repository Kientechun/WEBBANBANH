using BakeryShop.Data;
using BakeryShop.Models.Entities;
using BakeryShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Policy = "AdminOnly")]
public class CategoriesController(BakeryDbContext db, ISlugService slugService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await db.Categories.AsNoTracking().OrderBy(c => c.DisplayOrder).ToListAsync(cancellationToken));

    [HttpGet]
    public IActionResult Create() => View("Edit", new Category());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model, CancellationToken cancellationToken)
    {
        model.Slug = slugService.Create(model.Name);
        if (await db.Categories.AnyAsync(c => c.Slug == model.Slug, cancellationToken))
            ModelState.AddModelError(nameof(model.Name), "Tên danh mục này đã tồn tại.");
        if (!ModelState.IsValid) return View("Edit", model);
        db.Categories.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FindAsync([id], cancellationToken);
        return category is null ? NotFound() : View(category);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category model, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FindAsync([model.Id], cancellationToken);
        if (category is null) return NotFound();
        var slug = slugService.Create(model.Name);
        if (await db.Categories.AnyAsync(c => c.Id != model.Id && c.Slug == slug, cancellationToken))
            ModelState.AddModelError(nameof(model.Name), "Tên danh mục này đã tồn tại.");
        if (!ModelState.IsValid) return View(model);
        category.Name = model.Name; category.Slug = slug; category.Description = model.Description;
        category.DisplayOrder = model.DisplayOrder; category.Status = model.Status; category.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FindAsync([id], cancellationToken);
        if (category is null) return NotFound();
        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}

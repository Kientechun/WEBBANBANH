using BakeryShop.Data;
using BakeryShop.Models.Entities;
using BakeryShop.Services;
using BakeryShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Policy = "AdminOnly")]
public class ProductsController(BakeryDbContext db, ISlugService slugService, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken cancellationToken = default)
    {
        const int pageSize = 15;
        var query = db.Products.Include(p => p.Category).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Name.Contains(search));
        ViewBag.Search = search;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync(cancellationToken) / (double)pageSize);
        return View(await query.OrderByDescending(p => p.CreatedAt).Skip((Math.Max(1, page) - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken) => View("Edit", await PrepareAsync(new AdminProductViewModel(), cancellationToken));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminProductViewModel model, CancellationToken cancellationToken)
    {
        await ValidateImageAsync(model);
        if (!ModelState.IsValid) return View("Edit", await PrepareAsync(model, cancellationToken));
        var slug = slugService.Create(model.Name);
        if (await db.Products.AnyAsync(p => p.Slug == slug, cancellationToken)) slug += "-" + Guid.NewGuid().ToString("N")[..8];
        var entity = new Product { Name = model.Name, Slug = slug, CategoryId = model.CategoryId, Price = model.Price, SalePrice = model.SalePrice, Stock = model.Stock, Description = model.Description, Ingredients = model.Ingredients, IsFeatured = model.IsFeatured, IsNew = model.IsNew, IsBestseller = model.IsBestseller, Status = model.Status };
        entity.Image = await SaveImageAsync(model.ImageFile);
        db.Products.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var p = await db.Products.FindAsync([id], cancellationToken);
        if (p is null) return NotFound();
        return View(await PrepareAsync(new AdminProductViewModel { Id = p.Id, Name = p.Name, CategoryId = p.CategoryId, Price = p.Price, SalePrice = p.SalePrice, Stock = p.Stock, Description = p.Description, Ingredients = p.Ingredients, IsFeatured = p.IsFeatured, IsNew = p.IsNew, IsBestseller = p.IsBestseller, Status = p.Status, ExistingImage = p.Image }, cancellationToken));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminProductViewModel model, CancellationToken cancellationToken)
    {
        await ValidateImageAsync(model);
        var entity = await db.Products.FindAsync([model.Id], cancellationToken);
        if (entity is null) return NotFound();
        if (!ModelState.IsValid) return View(await PrepareAsync(model, cancellationToken));
        entity.Name = model.Name; entity.Slug = slugService.Create(model.Name) + "-" + model.Id;
        entity.CategoryId = model.CategoryId; entity.Price = model.Price; entity.SalePrice = model.SalePrice; entity.Stock = model.Stock;
        entity.Description = model.Description; entity.Ingredients = model.Ingredients; entity.IsFeatured = model.IsFeatured;
        entity.IsNew = model.IsNew; entity.IsBestseller = model.IsBestseller; entity.Status = model.Status; entity.UpdatedAt = DateTime.UtcNow;
        entity.Image = await SaveImageAsync(model.ImageFile) ?? entity.Image;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null) return NotFound();
        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminProductViewModel> PrepareAsync(AdminProductViewModel model, CancellationToken cancellationToken)
    {
        model.Categories = await db.Categories.OrderBy(c => c.Name).Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToListAsync(cancellationToken);
        return model;
    }

    private Task ValidateImageAsync(AdminProductViewModel model)
    {
        if (model.ImageFile is null) return Task.CompletedTask;
        var ext = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
        if (model.ImageFile.Length > 2 * 1024 * 1024 || !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext))
            ModelState.AddModelError(nameof(model.ImageFile), "Ảnh phải là JPG, PNG hoặc WebP và không quá 2MB.");
        return Task.CompletedTask;
    }

    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file is null) return null;
        var directory = Path.Combine(environment.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(directory);
        var name = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        await using var stream = System.IO.File.Create(Path.Combine(directory, name));
        await file.CopyToAsync(stream);
        return $"/uploads/products/{name}";
    }
}

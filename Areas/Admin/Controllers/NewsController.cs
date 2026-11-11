using BakeryShop.Data;
using BakeryShop.Models.Entities;
using BakeryShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Policy = "AdminOnly")]
public class NewsController(BakeryDbContext db, ISlugService slugService, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await db.News.AsNoTracking().OrderByDescending(n => n.CreatedAt).ToListAsync(cancellationToken));

    [HttpGet]
    public IActionResult Create() => View("Edit", new News());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(News model, IFormFile? imageFile, CancellationToken cancellationToken)
    {
        model.Slug = slugService.Create(model.Title);
        if (await db.News.AnyAsync(n => n.Slug == model.Slug, cancellationToken))
            ModelState.AddModelError(nameof(model.Title), "Tiêu đề này đã được sử dụng.");
        if (!ValidateImage(imageFile)) ModelState.AddModelError(nameof(imageFile), "Ảnh chỉ nhận JPG, PNG, WebP và tối đa 2MB.");
        if (!ModelState.IsValid) return View("Edit", model);
        model.Image = await SaveImageAsync(imageFile);
        db.News.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var news = await db.News.FindAsync([id], cancellationToken);
        return news is null ? NotFound() : View(news);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(News model, IFormFile? imageFile, CancellationToken cancellationToken)
    {
        var news = await db.News.FindAsync([model.Id], cancellationToken);
        if (news is null) return NotFound();
        var slug = slugService.Create(model.Title);
        if (await db.News.AnyAsync(n => n.Id != model.Id && n.Slug == slug, cancellationToken))
            ModelState.AddModelError(nameof(model.Title), "Tiêu đề này đã được sử dụng.");
        if (!ValidateImage(imageFile)) ModelState.AddModelError(nameof(imageFile), "Ảnh chỉ nhận JPG, PNG, WebP và tối đa 2MB.");
        if (!ModelState.IsValid) return View(model);
        news.Title = model.Title; news.Slug = slug; news.Summary = model.Summary; news.Content = model.Content;
        news.Author = model.Author; news.Status = model.Status; news.UpdatedAt = DateTime.UtcNow;
        news.Image = await SaveImageAsync(imageFile) ?? news.Image;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var item = await db.News.FindAsync([id], cancellationToken);
        if (item is null) return NotFound();
        db.News.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private static bool ValidateImage(IFormFile? file) => file is null ||
        (file.Length <= 2 * 1024 * 1024 && new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()));

    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file is null) return null;
        var directory = Path.Combine(environment.WebRootPath, "uploads", "news");
        Directory.CreateDirectory(directory);
        var name = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        await using var stream = System.IO.File.Create(Path.Combine(directory, name));
        await file.CopyToAsync(stream);
        return $"/uploads/news/{name}";
    }
}

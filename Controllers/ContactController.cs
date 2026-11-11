using BakeryShop.Data;
using BakeryShop.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BakeryShop.Controllers;

public class ContactController(BakeryDbContext db) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new Contact());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(Contact model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        model.Status = "new";
        db.Contacts.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Message"] = "Cảm ơn bạn đã liên hệ. Chúng tôi sẽ phản hồi sớm!";
        return RedirectToAction(nameof(Index));
    }
}

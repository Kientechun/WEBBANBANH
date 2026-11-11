using BakeryShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Policy = "AdminOnly")]
public class ContactsController(BakeryDbContext db) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await db.Contacts.AsNoTracking().OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken)
    {
        var contact = await db.Contacts.FindAsync([id], cancellationToken);
        return contact is null ? NotFound() : View(contact);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, CancellationToken cancellationToken)
    {
        if (status is not ("new" or "processing" or "completed")) return BadRequest("Trạng thái liên hệ không hợp lệ.");
        var contact = await db.Contacts.FindAsync([id], cancellationToken);
        if (contact is null) return NotFound();
        contact.Status = status;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var contact = await db.Contacts.FindAsync([id], cancellationToken);
        if (contact is null) return NotFound();
        db.Contacts.Remove(contact);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}

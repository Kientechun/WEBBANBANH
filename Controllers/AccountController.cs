using System.Security.Claims;
using BakeryShop.Data;
using BakeryShop.Models.Entities;
using BakeryShop.Services;
using BakeryShop.ViewModels;
using System.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace BakeryShop.Controllers;

public class AccountController(BakeryDbContext db, ICartService cart, ILogger<AccountController> logger, IWebHostEnvironment environment) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email, cancellationToken);
        if (user is null || !user.Status || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Name), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role) };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        await cart.MergeSessionCartAsync(user.Id, cancellationToken);
        return LocalRedirect(!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl : Url.Action("Index", "Home")!);
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.Email == model.Email, cancellationToken)) ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
        if (!ModelState.IsValid) return View(model);
        db.Users.Add(new User { Name = model.Name, Email = model.Email, Password = BCrypt.Net.BCrypt.HashPassword(model.Password), Role = "customer" });
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(string email)
    {
        logger.LogInformation("Password recovery requested for {Email}", email);
        TempData["Message"] = "Chức năng gửi email khôi phục chưa được cấu hình. Vui lòng liên hệ cửa hàng để xác minh và đặt lại mật khẩu.";
        return RedirectToAction(nameof(ForgotPassword));
    }

    [HttpGet, Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        ViewBag.Orders = await db.Orders.AsNoTracking().Where(o => o.UserId == id).OrderByDescending(o => o.CreatedAt).ToListAsync(cancellationToken);
        return View(await db.Users.FindAsync([id], cancellationToken));
    }

    [HttpGet, Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync([id], cancellationToken);
        if (user is null) return NotFound();
        return View(new ProfileViewModel { Name = user.Name, Phone = user.Phone, Address = user.Address, Avatar = user.Avatar });
    }

    [HttpPost, ValidateAntiForgeryToken, Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Profile(ProfileViewModel model, CancellationToken cancellationToken)
    {
        if (model.AvatarFile is not null && (model.AvatarFile.Length > 2 * 1024 * 1024 ||
            !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant())))
            ModelState.AddModelError(nameof(model.AvatarFile), "Ảnh chỉ nhận JPG, PNG, WebP và tối đa 2MB.");
        if (!ModelState.IsValid) return View(model);
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync([id], cancellationToken);
        if (user is null) return NotFound();
        user.Name = model.Name; user.Phone = model.Phone; user.Address = model.Address; user.UpdatedAt = DateTime.UtcNow;
        if (model.AvatarFile is not null)
        {
            var folder = Path.Combine(environment.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(folder);
            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant()}";
            await using var stream = System.IO.File.Create(Path.Combine(folder, fileName));
            await model.AvatarFile.CopyToAsync(stream, cancellationToken);
            user.Avatar = $"/uploads/avatars/{fileName}";
        }
        await db.SaveChangesAsync(cancellationToken);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Name), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role) }, CookieAuthenticationDefaults.AuthenticationScheme)));
        TempData["Message"] = "Đã cập nhật thông tin tài khoản.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost, ValidateAntiForgeryToken, Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync([id], cancellationToken);
        if (user is null) return NotFound();
        if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "Mật khẩu hiện tại không chính xác.");
            return View(model);
        }
        user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        TempData["Message"] = "Đã đổi mật khẩu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> CancelOrder(int id, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, cancellationToken);
        if (order is null) return NotFound();
        if (order.Status != "pending")
        {
            TempData["Message"] = "Chỉ có thể hủy đơn hàng đang chờ xác nhận.";
            return RedirectToAction(nameof(Index));
        }
        foreach (var item in order.Items)
        {
            if (item.ProductId is int productId)
            {
                var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
                if (product is not null) product.Stock += item.Quantity;
            }
        }
        order.Status = "cancelled";
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}

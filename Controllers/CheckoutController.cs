using System.Security.Claims;
using BakeryShop.Data;
using BakeryShop.Services;
using BakeryShop.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Controllers;

public class CheckoutController(IOrderService orders, ICartService cart, BakeryDbContext db, IVnPayService vnPay, ILogger<CheckoutController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var basket = await cart.GetAsync(cancellationToken);
        if (basket.Count == 0) return RedirectToAction("Index", "Cart");
        var model = new CheckoutViewModel { Total = basket.Total };
        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        {
            var customer = await db.Users.FindAsync([id], cancellationToken);
            if (customer is not null) { model.CustomerName = customer.Name; model.CustomerEmail = customer.Email; model.CustomerPhone = customer.Phone ?? ""; model.CustomerAddress = customer.Address ?? ""; }
        }
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model, CancellationToken cancellationToken)
    {
        var basket = await cart.GetAsync(cancellationToken);
        model.Total = basket.Total;
        if (!ModelState.IsValid || basket.Count == 0) return View(model);
        try
        {
            var order = await orders.CheckoutAsync(model, cancellationToken);
            HttpContext.Session.SetString("last_order", order.OrderNumber);
            if (order.PaymentMethod == "vnpay") return Redirect(vnPay.CreatePaymentUrl(order, HttpContext));
            return RedirectToAction(nameof(Thanks), new { orderNumber = order.OrderNumber });
        }
        catch (Exception ex) when (ex is InvalidOperationException or DbUpdateException)
        {
            logger.LogWarning(ex, "Unable to complete checkout");
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Thanks(string orderNumber, CancellationToken cancellationToken)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
        if (order is null) return NotFound();
        var isOwner = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) && order.UserId == uid;
        if (!isOwner && !User.IsInRole("admin") && HttpContext.Session.GetString("last_order") != orderNumber) return NotFound();
        return View(order);
    }

    [HttpGet]
    public async Task<IActionResult> VnPayReturn(CancellationToken cancellationToken)
    {
        if (!vnPay.ValidateReturn(Request.Query)) return BadRequest("Chữ ký thanh toán không hợp lệ.");
        var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == Request.Query["vnp_TxnRef"], cancellationToken);
        if (order is null) return NotFound();
        if (!long.TryParse(Request.Query["vnp_Amount"], out var amount) || amount != (long)(order.TotalAmount * 100))
            return BadRequest("Số tiền thanh toán không khớp đơn hàng.");
        if (Request.Query["vnp_ResponseCode"] == "00" && Request.Query["vnp_TransactionStatus"] == "00")
        {
            order.PaymentStatus = "paid"; order.TransactionId = Request.Query["vnp_TransactionNo"]; order.PaidAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
        HttpContext.Session.SetString("last_order", order.OrderNumber);
        return RedirectToAction(nameof(Thanks), new { orderNumber = order.OrderNumber });
    }

    [HttpGet]
    public async Task<IActionResult> VnPayIpn(CancellationToken cancellationToken)
    {
        if (!vnPay.ValidateReturn(Request.Query)) return Json(new { RspCode = "97", Message = "Invalid signature" });
        var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == Request.Query["vnp_TxnRef"], cancellationToken);
        if (order is null) return Json(new { RspCode = "01", Message = "Order not found" });
        if (!long.TryParse(Request.Query["vnp_Amount"], out var amount) || amount != (long)(order.TotalAmount * 100))
            return Json(new { RspCode = "04", Message = "Invalid amount" });
        if (Request.Query["vnp_ResponseCode"] == "00" && order.PaymentStatus != "paid")
        {
            order.PaymentStatus = "paid"; order.TransactionId = Request.Query["vnp_TransactionNo"]; order.PaidAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
        return Json(new { RspCode = "00", Message = "Confirm Success" });
    }
}

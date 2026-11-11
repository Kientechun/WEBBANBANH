using BakeryShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace BakeryShop.Helpers;

public sealed class CartCountViewComponent(ICartService cart) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var basket = await cart.GetAsync(HttpContext.RequestAborted);
        return View(basket.Count);
    }
}

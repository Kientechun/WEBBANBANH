using BakeryShop.Models.Entities;
using BakeryShop.ViewModels;

namespace BakeryShop.Services;

public interface IOrderService
{
    Task<Order> CheckoutAsync(CheckoutViewModel checkout, CancellationToken cancellationToken = default);
}

using BakeryShop.Models.Entities;

namespace BakeryShop.Services;

public interface IVnPayService
{
    string CreatePaymentUrl(Order order, HttpContext context);
    bool ValidateReturn(IQueryCollection query);
}

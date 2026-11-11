using BakeryShop.ViewModels;

namespace BakeryShop.Services;

public interface ICartService
{
    Task<CartViewModel> GetAsync(CancellationToken cancellationToken = default);
    Task<CartViewModel> SetQuantityAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    Task<CartViewModel> RemoveAsync(int productId, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
    Task MergeSessionCartAsync(int userId, CancellationToken cancellationToken = default);
}

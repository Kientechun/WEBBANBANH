using System.ComponentModel.DataAnnotations;
using BakeryShop.Models.Entities;

namespace BakeryShop.ViewModels;

public class HomeViewModel
{
    public IReadOnlyList<Product> Featured { get; init; } = [];
    public IReadOnlyList<Product> NewProducts { get; init; } = [];
    public IReadOnlyList<Product> Bestsellers { get; init; } = [];
    public IReadOnlyList<News> LatestNews { get; init; } = [];
    public IReadOnlyList<Category> Categories { get; init; } = [];
}

public class ProductListViewModel
{
    public IReadOnlyList<Product> Products { get; init; } = [];
    public IReadOnlyList<Category> Categories { get; init; } = [];
    public string? Search { get; init; }
    public int? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string Sort { get; init; } = "newest";
    public int Page { get; init; }
    public int TotalPages { get; init; }
}

public class CheckoutViewModel
{
    [Required, StringLength(120)] public string CustomerName { get; set; } = "";
    [Required, EmailAddress] public string CustomerEmail { get; set; } = "";
    [Required, Phone] public string CustomerPhone { get; set; } = "";
    [Required, StringLength(500)] public string CustomerAddress { get; set; } = "";
    [Required] public string PaymentMethod { get; set; } = "cod";
    [StringLength(1000)] public string? Notes { get; set; }
    public decimal Total { get; set; }
}

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}

public class CartLineViewModel
{
    public int ProductId { get; init; }
    public string Name { get; init; } = "";
    public string? Image { get; init; }
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public int Stock { get; init; }
    public decimal Subtotal => Price * Quantity;
}

public class CartViewModel
{
    public IReadOnlyList<CartLineViewModel> Items { get; init; } = [];
    public decimal Total => Items.Sum(x => x.Subtotal);
    public int Count => Items.Sum(x => x.Quantity);
}

public class ProfileViewModel
{
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [Phone, StringLength(30)] public string? Phone { get; set; }
    [StringLength(500)] public string? Address { get; set; }
    public IFormFile? AvatarFile { get; set; }
    public string? Avatar { get; set; }
}

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password)] public string CurrentPassword { get; set; } = "";
    [Required, MinLength(8), DataType(DataType.Password)] public string NewPassword { get; set; } = "";
    [Required, Compare(nameof(NewPassword)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}

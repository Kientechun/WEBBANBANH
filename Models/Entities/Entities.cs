using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BakeryShop.Models.Entities;

public class Category
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [Required, StringLength(140)] public string Slug { get; set; } = "";
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int DisplayOrder { get; set; }
    public bool Status { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    [Required, StringLength(180)] public string Name { get; set; } = "";
    [Required, StringLength(200)] public string Slug { get; set; } = "";
    public string Description { get; set; } = "";
    public string Ingredients { get; set; } = "";
    [Column(TypeName = "decimal(10,2)")] public decimal Price { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal? SalePrice { get; set; }
    public string? Image { get; set; }
    public string Gallery { get; set; } = "[]";
    public int Stock { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public bool IsBestseller { get; set; }
    public int Views { get; set; }
    public bool Status { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [NotMapped] public decimal EffectivePrice => SalePrice ?? Price;
}

public class User
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [Required, EmailAddress, StringLength(180)] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Avatar { get; set; }
    [Required] public string Role { get; set; } = "customer";
    public bool Status { get; set; } = true;
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}

public class Order
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    [Required] public string OrderNumber { get; set; } = "";
    [Required] public string CustomerName { get; set; } = "";
    [Required] public string CustomerEmail { get; set; } = "";
    [Required] public string CustomerPhone { get; set; } = "";
    [Required] public string CustomerAddress { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = "cod";
    public string PaymentStatus { get; set; } = "unpaid";
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public string Status { get; set; } = "pending";
    public string? Notes { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    public string ProductName { get; set; } = "";
    public string? ProductImage { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}

public class News
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string Title { get; set; } = "";
    [Required, StringLength(220)] public string Slug { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Content { get; set; } = "";
    public string? Image { get; set; }
    public string Author { get; set; } = "BakeryShop";
    public int Views { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Contact
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";
    public string Status { get; set; } = "new";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Setting
{
    public int Id { get; set; }
    [Required] public string SettingKey { get; set; } = "";
    public string SettingValue { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
}

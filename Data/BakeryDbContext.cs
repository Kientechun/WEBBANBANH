using BakeryShop.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Data;

public sealed class BakeryDbContext(DbContextOptions<BakeryDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<News> News => Set<News>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<CartItem> Cart => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
        model.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
        model.Entity<Product>().HasIndex(x => new { x.CategoryId, x.Status });
        model.Entity<Product>().HasIndex(x => x.IsFeatured);
        model.Entity<Product>().HasIndex(x => x.IsNew);
        model.Entity<User>().HasIndex(x => x.Email).IsUnique();
        model.Entity<Order>().HasIndex(x => x.OrderNumber).IsUnique();
        model.Entity<News>().HasIndex(x => x.Slug).IsUnique();
        model.Entity<Setting>().HasIndex(x => x.SettingKey).IsUnique();
        model.Entity<CartItem>().HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
        model.Entity<Product>().Property(x => x.Price).HasPrecision(10, 2);
        model.Entity<Product>().Property(x => x.SalePrice).HasPrecision(10, 2);
        model.Entity<Order>().Property(x => x.TotalAmount).HasPrecision(10, 2);
        model.Entity<OrderItem>().Property(x => x.Price).HasPrecision(10, 2);
        model.Entity<OrderItem>().Property(x => x.Subtotal).HasPrecision(10, 2);
        model.Entity<Product>().Property(x => x.Gallery).HasColumnType("json");
        model.Entity<Category>().HasMany(x => x.Products).WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<Order>().HasMany(x => x.Items).WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<Order>().HasOne(x => x.User).WithMany(x => x.Orders)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        model.Entity<OrderItem>().HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull);
        model.Entity<CartItem>().HasOne(x => x.User).WithMany(x => x.CartItems)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<CartItem>().HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        base.OnModelCreating(model);
    }
}

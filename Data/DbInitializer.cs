using BakeryShop.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(BakeryDbContext db)
    {
        if (!await db.Users.AnyAsync()) await SeedBaseAsync(db);
        await SyncProductsAsync(db);
    }

    private const string Placeholder = "/images/cake-placeholder.svg";

    private static async Task SeedBaseAsync(BakeryDbContext db)
    {
        db.Users.Add(new User { Name = "Quản trị viên", Email = "admin@bakery.com", Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "admin", Status = true, EmailVerified = true });
        var categories = new[]
        {
            new Category { Name = "Bánh Ngọt", Slug = "banh-ngot", DisplayOrder = 1 },
            new Category { Name = "Bánh Mì", Slug = "banh-mi", DisplayOrder = 2 },
            new Category { Name = "Bánh Kem", Slug = "banh-kem", DisplayOrder = 3 },
            new Category { Name = "Bánh Quy", Slug = "banh-quy", DisplayOrder = 4 }
        };
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();
        db.News.AddRange(
            new News { Title = "Bí quyết giữ bánh tươi ngon", Slug = "bi-quyet-giu-banh-tuoi-ngon", Summary = "Cách bảo quản bánh đúng cách tại nhà.", Content = "Bảo quản bánh trong hộp kín và dùng trong ngày để thưởng thức hương vị tốt nhất.", Status = true },
            new News { Title = "Nguyên liệu tươi mỗi ngày", Slug = "nguyen-lieu-tuoi-moi-ngay", Summary = "Chúng tôi chọn nguyên liệu chất lượng.", Content = "BakeryShop ưu tiên nguyên liệu tươi, nguồn gốc rõ ràng.", Status = true },
            new News { Title = "Ưu đãi bánh ngọt cuối tuần", Slug = "uu-dai-banh-ngot-cuoi-tuan", Summary = "Khám phá các món bánh được yêu thích.", Content = "Theo dõi cửa hàng để cập nhật ưu đãi mới.", Status = true });
        db.Settings.AddRange(
            new Setting { SettingKey = "shop_name", SettingValue = "BakeryShop" },
            new Setting { SettingKey = "hotline", SettingValue = "0900000000" },
            new Setting { SettingKey = "address", SettingValue = "123 Đường Bánh Ngọt, TP. Hồ Chí Minh" },
            new Setting { SettingKey = "email", SettingValue = "hello@bakery.com" });
        await db.SaveChangesAsync();
    }

    // Sản phẩm mẫu có ảnh thật trong wwwroot/uploads/products
    private static readonly (string Name, string Slug, string Cat, decimal Price, int Stock, string Image, string Description)[] SeedProducts =
    {
        ("Bánh Ốc Quế Kem", "banh-oc-que-kem", "banh-ngot", 15000, 40, "banh-oc-que-kem.jpg", "Bánh ốc quế giòn tan với kem mát lạnh."),
        ("Tiramisu", "tiramisu", "banh-ngot", 37000, 20, "tiramisu.jpg", "Tiramisu mềm mịn vị cà phê đặc trưng."),
        ("Coconut Mochi", "coconut-mochi", "banh-ngot", 16000, 30, "coconut-mochi.jpg", "Bánh mochi dừa mềm dẻo."),
        ("Su kem khoai môn", "su-kem-khoai-mon", "banh-ngot", 18000, 50, "su_kem_khoai_mon.jpg", "Vỏ su mềm cùng nhân kem khoai môn béo ngậy."),
        ("Bánh gato cuộn anh đào", "banh-gato-cuon-anh-dao", "banh-ngot", 68000, 15, "banh_gato_cuon_anh_dao.jpg", "Bánh bông lan cuộn kem và anh đào."),
        ("Bánh cuộn trà xanh", "banh-cuon-tra-xanh", "banh-ngot", 68000, 15, "banh-cuon-tra-xanh.png", "Bánh bông lan cuộn kem trà xanh."),
        ("Hộp bánh trung thu Lam Nguyệt", "hop-banh-trung-thu-lam-nguyet", "banh-ngot", 450000, 10, "lam-nguyet.jpg", "Hộp quà trung thu 6 bánh nhiều hương vị."),
        ("Bánh Croissant", "banh-croissant", "banh-mi", 17000, 30, "croissant.jpg", "Bánh sừng bò giòn xốp kiểu Pháp."),
        ("Bánh mì ruốc heo", "banh-mi-ruoc-heo", "banh-mi", 13000, 30, "floss-pork-bread.jpg", "Bánh mì mềm phủ ruốc heo thơm ngon."),
        ("Bánh mì Hàn Quốc mè đen", "banh-mi-han-quoc-me-den", "banh-mi", 25000, 25, "banh-my-han-quoc-me-den.png", "Bánh mì kiểu Hàn phủ mè đen thơm bùi."),
        ("White Cheese and Caramel Cake", "white-cheese-caramel-cake", "banh-kem", 335000, 8, "white-cheese-cake.jpg", "Bánh kem phô mai trắng và caramel."),
        ("Banana Cheese Cake", "banana-cheese-cake", "banh-kem", 370000, 8, "banana-cheese-cake.jpg", "Bánh kem chuối phô mai."),
        ("Madeleine chocolate", "madeleine-chocolate", "banh-quy", 42000, 25, "banh-madeleine-chocolate.png", "Bánh quy bơ hình sò vị chocolate.")
    };

    // Thay các sản phẩm mẫu cũ (ảnh giữ chỗ) bằng sản phẩm có ảnh thật. Chạy một lần.
    private static async Task SyncProductsAsync(BakeryDbContext db)
    {
        var placeholders = await db.Products.Where(p => p.Image == Placeholder).ToListAsync();
        var oldSeed = placeholders.Where(p => System.Text.RegularExpressions.Regex.IsMatch(p.Slug, @"^\d+-")).ToList();
        if (oldSeed.Count == 0 && await db.Products.AnyAsync()) return;
        db.Products.RemoveRange(oldSeed);
        var categories = await db.Categories.ToDictionaryAsync(c => c.Slug);
        var existing = (await db.Products.Select(p => p.Slug).ToListAsync()).ToHashSet();
        var i = 0;
        foreach (var p in SeedProducts)
        {
            i++;
            if (existing.Contains(p.Slug) || !categories.TryGetValue(p.Cat, out var category)) continue;
            db.Products.Add(new Product
            {
                Name = p.Name, Slug = p.Slug, CategoryId = category.Id, Price = p.Price, Stock = p.Stock,
                Description = p.Description, Ingredients = "Bột mì, bơ, trứng, đường và nguyên liệu tươi.",
                Image = "/uploads/products/" + p.Image, IsFeatured = i <= 6, IsNew = i > 6, IsBestseller = i % 2 == 1, Status = true
            });
        }
        await db.SaveChangesAsync();
    }

    private static string Slug(string text) => text.ToLowerInvariant().Replace(' ', '-');
}

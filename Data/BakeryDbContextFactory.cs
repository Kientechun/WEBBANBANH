using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BakeryShop.Data;

public sealed class BakeryDbContextFactory : IDesignTimeDbContextFactory<BakeryDbContext>
{
    public BakeryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BakeryDbContext>()
            .UseMySql("Server=localhost;Port=3306;Database=bakery_shop;User=root;Password=CHANGE_ME;CharSet=utf8mb4",
                new MySqlServerVersion(new Version(8, 0, 36))).UseSnakeCaseNamingConvention()
            .Options;
        return new BakeryDbContext(options);
    }
}

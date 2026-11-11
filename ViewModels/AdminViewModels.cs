using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BakeryShop.ViewModels;

public class AdminProductViewModel
{
    public int Id { get; set; }
    [Required, StringLength(180)] public string Name { get; set; } = "";
    [Required] public int CategoryId { get; set; }
    [Required, Range(0, 100000000)] public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    [Range(0, 100000)] public int Stock { get; set; }
    public string Description { get; set; } = "";
    public string Ingredients { get; set; } = "";
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public bool IsBestseller { get; set; }
    public bool Status { get; set; } = true;
    public IFormFile? ImageFile { get; set; }
    public string? ExistingImage { get; set; }
    public IEnumerable<SelectListItem> Categories { get; set; } = [];
}

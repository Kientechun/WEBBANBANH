using BakeryShop.Services;
using BakeryShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace BakeryShop.Controllers;

public class HomeController(IProductService products) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await products.GetHomeAsync(cancellationToken));
    public IActionResult About() => View("Static", ("Giới thiệu", "BakeryShop mang đến những chiếc bánh tươi ngon, được làm thủ công mỗi ngày từ nguyên liệu chất lượng."));
    public IActionResult Returns() => View("Static", ("Chính sách đổi trả", "Vui lòng liên hệ cửa hàng trong vòng 24 giờ nếu sản phẩm gặp vấn đề về chất lượng."));
    public IActionResult PrivacyPolicy() => View("Static", ("Chính sách bảo mật", "Thông tin cá nhân của khách hàng chỉ được sử dụng để xử lý đơn hàng và hỗ trợ dịch vụ."));
    public IActionResult Payments() => View("Static", ("Chính sách thanh toán", "Hỗ trợ thanh toán khi nhận hàng (COD) và thanh toán trực tuyến qua VNPAY."));
    public IActionResult FoodSafety() => View("Static", ("An toàn thực phẩm", "Sản phẩm được chế biến trong môi trường sạch sẽ và bảo quản theo quy định an toàn thực phẩm."));

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
}

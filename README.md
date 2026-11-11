# WEBBANBANH

WEBBANBANH là một website bán bánh trực tuyến được xây dựng bằng ASP.NET Core MVC. Dự án tập trung vào trải nghiệm mua hàng, quản lý giỏ hàng, thanh toán và quyền quản trị cửa hàng.

## Tính năng chính

- Hiển thị danh sách sản phẩm và danh mục bánh theo nhiều nhóm.
- Xem chi tiết sản phẩm, bình luận/đánh giá (nếu có trong luồng UI), và quản lý giỏ hàng.
- Đăng ký, đăng nhập, quản lý tài khoản người dùng.
- Đặt hàng và thanh toán qua VNPay sandbox.
- Quản lý đơn hàng và trạng thái thanh toán.
- Khu vực quản trị với quyền dành cho admin.
- Quản lý bài viết/news và liên hệ khách hàng.

## Công nghệ sử dụng

- C# / ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- Pomelo.EntityFrameworkCore.MySql
- MySQL 8
- Razor Views, HTML, CSS, JavaScript

## Yêu cầu hệ thống

- .NET 8 SDK
- MySQL Server 8+
- Visual Studio / VS Code / Rider

## Cài đặt

1. Clone repository:

```bash
git clone https://github.com/Kientechun/WEBBANBANH.git
cd WEBBANBANH
```

2. Tạo cơ sở dữ liệu MySQL và import schema từ file:

```bash
database/bakery_shop.sql
```

3. Cấu hình chuỗi kết nối trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=bakery_shop_cs;User=root;Password=;CharSet=utf8mb4"
  },
  "VnPay": {
    "TmnCode": "",
    "HashSecret": "",
    "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://localhost:5001/Checkout/VnPayReturn"
  }
}
```

4. Restore dependencies và chạy ứng dụng:

```bash
dotnet restore
dotnet run
```

5. Mở địa chỉ local hiển thị trong terminal để truy cập website.

## Cấu trúc thư mục

- `Controllers/`: xử lý request và logic màn hình.
- `Views/`: giao diện Razor MVC.
- `Models/` và `ViewModels/`: dữ liệu nghiệp vụ và model hiển thị.
- `Data/`: DbContext và khởi tạo dữ liệu.
- `Services/`: xử lý sản phẩm, giỏ hàng, đơn hàng, VNPay.
- `Areas/`: mô-đun admin.
- `wwwroot/`: tài nguyên tĩnh (CSS, JS, hình ảnh).
- `database/`: schema MySQL.

## Quy trình thanh toán

Dự án tích hợp VNPay để hỗ trợ thanh toán thử nghiệm trong môi trường sandbox. Để chạy đúng, bạn cần:

- điền `TmnCode` và `HashSecret` hợp lệ
- cấu hình `ReturnUrl` phù hợp với localhost hoặc môi trường triển khai
- không lưu thông tin nhạy cảm vào repository công khai

## Lưu ý bảo mật

- Không commit thông tin database thật, mật khẩu hoặc secret vào GitHub.
- Nên dùng biến môi trường hoặc User Secrets cho môi trường triển khai.
- Hãy bật HTTPS và kiểm tra quyền truy cập admin trước khi đưa lên production.

## Ghi chú

Dự án này phù hợp để làm mẫu website thương mại điện tử cơ bản, kết hợp MVC + database + chức năng bán hàng thực tế. Nếu bạn muốn, có thể tiếp tục mở rộng bằng thêm đánh giá sản phẩm, mã giảm giá, quản lý đơn hàng theo status, hoặc triển khai phân quyền mạnh hơn.

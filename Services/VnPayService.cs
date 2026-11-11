using System.Net;
using System.Security.Cryptography;
using System.Text;
using BakeryShop.Models.Entities;

namespace BakeryShop.Services;

public sealed class VnPayService(IConfiguration configuration) : IVnPayService
{
    public string CreatePaymentUrl(Order order, HttpContext context)
    {
        var settings = configuration.GetSection("VnPay");
        var secret = settings["HashSecret"];
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(settings["TmnCode"]))
            throw new InvalidOperationException("VNPAY chưa được cấu hình trong appsettings.json.");
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Amount"] = ((long)(order.TotalAmount * 100)).ToString(),
            ["vnp_Command"] = "pay", ["vnp_CreateDate"] = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND", ["vnp_IpAddr"] = GetClientIp(context),
            ["vnp_Locale"] = "vn", ["vnp_OrderInfo"] = $"Thanh toan don hang {order.OrderNumber}",
            ["vnp_OrderType"] = "other", ["vnp_ReturnUrl"] = settings["ReturnUrl"] ?? "",
            ["vnp_TmnCode"] = settings["TmnCode"] ?? "", ["vnp_TxnRef"] = order.OrderNumber,
            ["vnp_Version"] = "2.1.0"
        };
        var query = string.Join("&", values.Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));
        var signature = Convert.ToHexString(HMACSHA512.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(query))).ToLowerInvariant();
        return $"{settings["Url"]}?{query}&vnp_SecureHash={signature}";
    }

    public bool ValidateReturn(IQueryCollection query)
    {
        var secret = configuration["VnPay:HashSecret"];
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(query["vnp_SecureHash"])) return false;
        var values = query.Where(kv => kv.Key.StartsWith("vnp_", StringComparison.Ordinal) && kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
            .OrderBy(kv => kv.Key, StringComparer.Ordinal);
        var data = string.Join("&", values.Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value.ToString())}"));
        var computed = Convert.ToHexString(HMACSHA512.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computed), Encoding.UTF8.GetBytes(query["vnp_SecureHash"].ToString().ToLowerInvariant()));
    }

    private static string GetClientIp(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress;
        if (ip is null || IPAddress.IsLoopback(ip)) return "127.0.0.1";
        return (ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip).ToString();
    }
}

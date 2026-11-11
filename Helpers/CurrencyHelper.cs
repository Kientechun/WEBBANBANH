using System.Globalization;

namespace BakeryShop.Helpers;

public static class CurrencyHelper
{
    public static string ToVnd(this decimal amount) => amount.ToString("#,##0", CultureInfo.GetCultureInfo("vi-VN")) + "đ";
}

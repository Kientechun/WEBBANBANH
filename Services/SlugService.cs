using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BakeryShop.Services;

public sealed class SlugService : ISlugService
{
    public string Create(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var result = new StringBuilder();
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue;
            result.Append(character == 'đ' ? 'd' : character == 'Đ' ? 'D' : character);
        }
        return Regex.Replace(result.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
    }
}

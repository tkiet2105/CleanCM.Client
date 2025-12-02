using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CleanCCM.Domain.Exceptions;

public static class StringExtensions
{
    /// <summary>
    /// Chuyển chuỗi tiếng Việt thành slug URL thân thiện.
    /// </summary>
    public static string ToSlug(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // B1: Chuẩn hóa Unicode dạng tổ hợp
        string normalized = input.Normalize(NormalizationForm.FormD);

        // B2: Xóa dấu (NonSpacingMark)
        var builder = new StringBuilder();
        foreach (char c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }

        // B3: Chuẩn hóa lại thành FormC
        string noAccent = builder.ToString().Normalize(NormalizationForm.FormC);

        // B4: Xử lý chữ "đ"
        noAccent = noAccent
            .Replace("đ", "d")
            .Replace("Đ", "d");

        // B5: lowercase
        noAccent = noAccent.ToLowerInvariant();

        // B6: bỏ ký tự không hợp lệ
        noAccent = Regex.Replace(noAccent, @"[^a-z0-9\s-]", "");

        // B7: khoảng trắng → "-"
        noAccent = Regex.Replace(noAccent, @"\s+", "-");

        // B8: bỏ dấu "-" dư
        noAccent = Regex.Replace(noAccent, "-{2,}", "-").Trim('-');

        return noAccent;
    }
}
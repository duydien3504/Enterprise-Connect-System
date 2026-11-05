using System.Text.RegularExpressions;

namespace EnterpriseConnect.MessagingService.Infrastructure.Services.Ai;

public static class VietnameseProfanityMatcher
{
    private static readonly Regex HighSeverityRegex = new(
        @"(?i)\b(đ[iị]t|đ[ụu]|l[ồo]n|c[ặa]c|bu[ồo]i|d[áa]i|c[ặa]t|ph[òo]|đ[ĩi]|cave|g[áa]i\s*g[ọo]i|b[áa]n\s*hoa|m[ẹe]\s*m[àa]y|b[ốo]\s*m[àa]y|c[ụu]\s*m[àa]y|b[àa]\s*m[àa]y|m[ẹe]\s*ki[ếe]p|tiên\s*sư|tổ\s*sư|dm|dkm|dcm|đm|đkm|đcm|đmm|vcl|vkl|vl|clgt|cc|loz)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex MediumSeverityRegex = new(
        @"(?i)\b((th[ằa]ng|đ[ồo]|l[ũu]|b[ọo]n)\s*ch[óo]|ch[óo]\s*(ch[ếe]t|m[áa]|đ[ẻe]|hoang|d[ạa]i)|[óo]c\s*(ch[óo]|l[ợo]n|b[òo])|m[ặa]t\s*(ch[óo]|l[ợo]n|d[àa]y)|con\s*ch[óo]\s*(n[àa]y|kia|đ[ẻe])|s[úu]c\s*v[ậa]t|c[ầa]m\s*th[úu]|qu[áa]i\s*thai|kh[ốo]n\s*n[ạa]n|m[ấa]t\s*d[ạa]y|v[ôo]\s*h[ọo]c|đ[ốo]n\s*m[ạa]t|th[ằa]ng\s*ngu|con\s*ngu|ngu\s*(si|d[ốo]t|l[ồo]n|vcl|vkl|vl|xuẩn|ng[ốo]c)|n[ãa]o\s*tàn|b[ạa]i\s*n[ãa]o|thi[ểe]u\s*n[ăa]ng|đ[eé]o|đ[ếe]ch)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static (bool IsMatched, string Severity, string Reason) Match(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return (false, "None", string.Empty);
        }

        var highMatch = HighSeverityRegex.Match(content);
        if (highMatch.Success)
        {
            return (true, "High", $"Phát hiện từ ngữ thô tục, xúc phạm nghiêm trọng: \"{highMatch.Value}\"");
        }

        var medMatch = MediumSeverityRegex.Match(content);
        if (medMatch.Success)
        {
            return (true, "Medium", $"Phát hiện từ ngữ xúc phạm, miệt thị nhân phẩm: \"{medMatch.Value}\"");
        }

        return (false, "None", string.Empty);
    }
}

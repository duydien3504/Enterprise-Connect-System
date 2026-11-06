namespace EnterpriseConnect.ApiGateway.Extensions;

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseGatewaySecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            // Chống MIME-sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // Chống Clickjacking (ngăn chặn nhúng Gateway vào iframe)
            headers["X-Frame-Options"] = "DENY";

            // Kiểm soát thông tin Referer khi chuyển trang
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Bộ lọc XSS trên các trình duyệt cũ
            headers["X-XSS-Protection"] = "1; mode=block";

            // HSTS: Bắt buộc dùng HTTPS khi kết nối bảo mật
            if (context.Request.IsHttps)
            {
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }

            await next();
        });
    }
}

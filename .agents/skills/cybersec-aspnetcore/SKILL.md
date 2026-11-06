---
name: cybersec-aspnetcore
description: "Comprehensive cybersecurity standard and practical enforcement skill for ASP.NET Core Web API based on CyberSecAppStandard.md: Authentication, Authorization, OWASP Top 10, Applied Cryptography, Data Protection, API Security, Safe Logging, Audit Trail, and DevSecOps."
---

# Cybersecurity Application Standard for ASP.NET Core Web API

## Mục Đích & Phạm Vi
Kỹ năng này xác lập và thực thi bộ tiêu chuẩn kỹ năng an toàn thông tin (Cybersecurity Application Standard) áp dụng trực tiếp cho nền tảng backend **ASP.NET Core Web API (.NET 8/9)** trong dự án **EnterpriseConnect**.

Tài liệu tham chiếu chuẩn: [CyberSecAppStandard.md](file:///e:/ChatEnterprise/EnterpriseConnect/Docs/CyberSecAppStandard.md).

---

## 1. Nguyên Tắc Bảo Mật Cốt Lõi (Core Invariants)

1. **Fail-Secure (Fail Closed) & Least Privilege:**
   - Mặc định Controller và Minimal API endpoint đều phải được bảo vệ bởi `[Authorize]`.
   - `[AllowAnonymous]` chỉ được áp dụng khi có lý do kỹ thuật rõ ràng (ví dụ: login, verify-email, health check) và phải có chú thích lý do.
   - Luôn kiểm soát quyền truy cập theo nguyên tắc quyền tối thiểu (Least Privilege).

2. **Mật Mã Học Ứng Dụng (Applied Cryptography):**
   - **Sinh số ngẫu nhiên:** Tuyệt đối không dùng `new Random()` cho bảo mật. Luôn dùng `RandomNumberGenerator.GetBytes(length)`.
   - **Hash mật khẩu:** Bắt buộc dùng `PasswordHasher<T>` (PBKDF2) hoặc `BCrypt.Net-Next` với salt ngẫu nhiên. Tuyệt đối không dùng MD5, SHA-1 hoặc SHA-256 trần cho mật khẩu.
   - **Mã hóa đối xứng:** Sử dụng **AES-256-GCM** (`System.Security.Cryptography.AesGcm`) với nonce 12 bytes ngẫu nhiên và authentication tag 16 bytes. Tuyệt đối tránh ECB mode hoặc CBC không có MAC.
   - **So sánh an toàn:** So sánh chuỗi bí mật/token bằng hàm constant-time:
     ```csharp
     CryptographicOperations.FixedTimeEquals(hashA, hashB);
     ```

3. **Quản Lý Bí Mật (Secrets Management):**
   - Không hardcode ConnectionStrings, JWT Secret Key, API Keys trong mã nguồn hoặc `appsettings.json` khi đẩy lên VCS.
   - Môi trường Local: Dùng .NET Secret Manager (`dotnet user-secrets`).
   - Môi trường Server/Production: Đọc từ biến môi trường (Environment Variables) hoặc Azure Key Vault.

---

## 2. Xác Thực & Phiên Làm Việc (Authentication)

1. **Cấu Hình JWT Bearer Chuẩn:**
   - Bắt buộc kiểm tra `ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ValidateIssuerSigningKey`.
   - Đặt `ClockSkew = TimeSpan.Zero` (hoặc tối đa `TimeSpan.FromMinutes(1)`) để ngăn chặn việc tái sử dụng token sau khi đã hết hạn.

2. **Hỗ Trợ SignalR WebSockets Qua API Gateway:**
   - Trình duyệt không thể gửi header `Authorization: Bearer` khi nâng cấp WebSocket (`new WebSocket()`).
   - Cả Gateway và Microservice phải đồng bộ xử lý sự kiện `OnMessageReceived`:
     ```csharp
     options.Events = new JwtBearerEvents
     {
         OnMessageReceived = context =>
         {
             var accessToken = context.Request.Query["access_token"];
             var path = context.HttpContext.Request.Path;
             if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
             {
                 context.Token = accessToken;
             }
             return Task.CompletedTask;
         }
     };
     ```

3. **Quản Lý Refresh Token & Thu Hồi Phiên (Revocation):**
   - Refresh token phải được lưu trữ an toàn trong DB kèm `ExpiresAt`, `RevokedAt`, `ReplacedByToken`.
   - Áp dụng cơ chế **Refresh Token Rotation**: Mỗi khi dùng Refresh Token để lấy Access Token mới, hủy token cũ và phát hành token mới. Nếu phát hiện token cũ bị sử dụng lại, lập tức thu hồi toàn bộ phiên của người dùng (Token Reuse Detection).

---

## 3. Phân Quyền (Authorization) & Chống IDOR/BOLA

1. **Phân Quyền Theo Role & Policy:**
   - Sử dụng Policy-based Authorization (`[Authorize(Policy = "...")]`) thay vì hardcode tên Role trong controller để dễ mở rộng và bảo trì.

2. **Resource-Based Authorization (Kiểm Soát Sở Hữu Dữ Liệu):**
   - **Nguy cơ BOLA/IDOR (Broken Object Level Authorization):** Kẻ tấn công sửa ID trong URL (ví dụ: `GET /api/Conversations/{id}`) để xem dữ liệu của người khác.
   - **Khắc phục chuẩn:** Luôn kiểm tra quyền sở hữu tại Handler/Service trước khi thực hiện nghiệp vụ:
     ```csharp
     var conversation = await _uow.Conversations.GetByIdWithMembersAsync(request.ConversationId, cancellationToken)
         ?? throw new KeyNotFoundException("Không tìm thấy cuộc trò chuyện.");

     var isMember = conversation.Members.Any(m => m.UserId == currentUserId);
     if (!isMember && !currentUser.IsAdmin)
     {
         throw new UnauthorizedAccessException("Bạn không có quyền truy cập dữ liệu này.");
     }
     ```

---

## 4. Bảo Mật Ứng Dụng Web (OWASP Top 10)

1. **Chống SQL Injection:**
   - Sử dụng 100% EF Core LINQ parameterized queries.
   - Tuyệt đối không nối chuỗi hoặc dùng string interpolation `$""` trực tiếp trong `FromSqlRaw()`. Nếu buộc phải dùng SQL thô, dùng `FromSqlInterpolated()` hoặc truyền `NpgsqlParameter` / `SqlParameter`.

2. **Chống Mass Assignment (Over-Posting):**
   - Không bind trực tiếp Database Entity vào Controller action parameters.
   - Luôn sử dụng DTO riêng biệt cho Input (`CreateRequest`, `UpdateRequest`).

3. **Chống Server-Side Request Forgery (SSRF):**
   - Khi gọi external URL từ backend (webhook, crawl dữ liệu, preview link), bắt buộc kiểm tra IP đích.
   - Chặn tuyệt đối các dải IP nội bộ: Loopback (`127.0.0.0/8`, `::1`), Private IP (`10.0.0.0/8`, `172.16.0.0/12`, `192.168.0.0/16`), Link-local (`169.254.0.0/16`).

4. **Bảo Mật File Upload:**
   - Kiểm tra định dạng theo Whitelist đuôi file (.pdf, .docx, .png, .jpg).
   - Kiểm tra Magic Bytes thực tế của file, không chỉ tin tưởng `ContentType` từ client.
   - Giới hạn dung lượng tải lên qua `[RequestSizeLimit]`.
   - Sinh tên file ngẫu nhiên bằng GUID, lưu trữ trên Object Storage (Cloudinary/S3/Blob Storage) hoặc thư mục cách ly không có quyền execute script.

---

## 5. Bảo Mật API & Cổng Gateway

1. **Chính Sách Giới Hạn Tốc Độ (Rate Limiting):**
   - Áp dụng phân tầng trên API Gateway:
     - Policy `auth-strict`: 20 requests/phút cho các endpoint đăng nhập, cấp lại token, đổi mật khẩu (chống brute-force và spam OTP).
     - Policy `global`: 300 requests/phút cho các API nghiệp vụ thông thường.
   - Trả về mã chuẩn `429 Too Many Requests` kèm JSON giải thích.

2. **Bảo Vệ Endpoint Nội Bộ (Zero Trust Shielding):**
   - Các API chỉ phục vụ giao tiếp giữa các microservices (như `GET /api/Contacts/check-block`) phải được chặn ngay tại Gateway với mã `403 Forbidden` khi có yêu cầu gọi từ bên ngoài.

3. **Xử Lý Lỗi An Toàn (Information Leakage):**
   - Tuyệt đối không để lộ Stack Trace, tên bảng, truy vấn SQL, hoặc exception nội bộ ra client trong môi trường Production.
   - Định dạng toàn bộ lỗi theo chuẩn **RFC 7807 ProblemDetails**.

---

## 6. Ghi Log An Toàn & Dấu Chân Điện Tử (Safe Logging & Audit Trail)

1. **Xóa Dữ Liệu Nhạy Cảm (PII & Secret Redaction):**
   - Không bao giờ ghi vào log: Mật khẩu plaintext, Access Token, Refresh Token, Số thẻ thanh toán, OTP.
   - Cấu hình Serilog `Destructure` hoặc middleware làm sạch body trước khi log.

2. **Audit Logs Truy Vết Chi Tiết (Cyber Forensics):**
   - Mọi hành động nhạy cảm (thay đổi quyền, chỉnh sửa tài liệu, khóa tài khoản, truy cập dữ liệu quan trọng) phải được lưu vết vào bảng `AuditLogs`:
     - `CorrelationId`: Mã định danh luồng yêu cầu xuyên suốt từ Gateway tới các Service.
     - `ServiceName`, `ActionName`, `TargetEntity`, `TargetId`.
     - `ActorId`, `ActorEmail`, `ActorRole`.
     - `IpAddress`, `UserAgent`, `HttpMethod`, `EndpointUrl`.
     - `StatusCode`, `ExecutionDurationMs`, `RiskLevel` (Low, Medium, High, Critical).

---

## 7. Quy Trình Kiểm Tra Bảo Mật Khi Viết Code (Pre-Commit Security Checklist)

Khi triển khai bất kỳ tính năng mới hoặc refactor endpoint, thực hiện rà soát theo 7 câu hỏi:
- [ ] Endpoint này ai được gọi? Đã có `[Authorize]` chưa?
- [ ] Tham số đầu vào có được validate chặt chẽ bằng DataAnnotations hoặc FluentValidation chưa?
- [ ] Có nguy cơ IDOR/BOLA không? Đã kiểm tra quyền sở hữu bản ghi chưa?
- [ ] Dữ liệu trả về có bị lộ PII, password hash, hay thông tin nhạy cảm không?
- [ ] Có câu lệnh SQL nào dùng phép cộng chuỗi hoặc nội suy không an toàn không?
- [ ] Có ghi log mật khẩu hoặc token không?
- [ ] Header `X-Correlation-Id` có được chuyển tiếp xuyên suốt luồng gọi API không?

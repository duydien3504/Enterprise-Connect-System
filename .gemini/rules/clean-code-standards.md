---
description: "Tiêu chuẩn tổ chức cấu trúc file, Single Responsibility Principle (SRP) và Clean Architecture cho EnterpriseConnect"
globs: ["Services/**", "BuildingBlocks/**", "EnterpriseConnect.ApiGateway/**"]
---

# Enterprise Clean Code & File Structure Standards

Quy chuẩn này là các nguyên tắc bắt buộc (Guardrails) nhằm loại bỏ anti-pattern "God File", duy trì tính module hóa cao và tuân thủ nghiêm ngặt nguyên lý SOLID.

---

## 1. Giới hạn số dòng code (Line Count Thresholds)

- **`Program.cs`**: **Tối đa 50 dòng**. 
  - `Program.cs` chỉ đóng vai trò là "Nhạc trưởng điều phối" (Orchestrator).
  - Không viết trực tiếp các khối lambda cấu hình dài (như CORS, JWT, RateLimiter, ReverseProxy, Swagger). Toàn bộ phải được tách thành các **Extension Methods** tương ứng trong thư mục `Extensions/`.
- **Controllers**: **Tối đa 120 dòng/file**.
  - Không nhồi nhét quá nhiều Action vào một file controller.
  - Sử dụng **Partial Class theo Feature** (ví dụ: `AuthController.Login.cs`, `AuthController.Sessions.cs`) hoặc **Granular Controllers** để mỗi file chỉ tập trung vào một nhóm tác vụ nhất định mà vẫn bảo toàn 100% Route URL (`api/[controller]`).
- **MediatR Commands, Queries & Handlers**: **Mỗi file một mục đích duy nhất**.
  - Không gộp nhiều Command/Query khác nhau vào chung một file `Commands.cs` dài hàng trăm dòng.
  - Phân tách rõ ràng:
    - `[FeatureName]Command.cs` (hoặc `Query.cs`)
    - `[FeatureName]CommandHandler.cs` (hoặc `QueryHandler.cs`)
    - `[FeatureName]CommandValidator.cs` (nếu có)
- **Services & Repositories**: **Tối đa 150 dòng/file**. Nếu vượt quá, phải phân rã thành các helper hoặc sub-services chuyên biệt theo SRP.

---

## 2. Quy ước tổ chức thư mục & Phân tách trách nhiệm

### A. Tầng Khởi tạo & Cấu hình (`Api` project)
Mỗi dự án API phải có thư mục `Extensions/` chứa:
- `CorsExtensions.cs`: Cấu hình chính sách CORS.
- `AuthenticationExtensions.cs`: Cấu hình JWT Bearer và Authorization Policies.
- `SwaggerExtensions.cs`: Cấu hình OpenAPI Docs, Security Schemes.
- `RateLimitingExtensions.cs`: Cấu hình phân vùng Rate Limit.
- `ReverseProxyExtensions.cs`: Cấu hình YARP Reverse Proxy (nếu là Gateway).

### B. Tầng Giao tiếp (`Controllers`)
- Controller chỉ làm nhiệm vụ: **(1)** Nhận request ➔ **(2)** Validate sơ bộ ➔ **(3)** Ủy quyền cho MediatR `_mediator.Send()` ➔ **(4)** Trả HTTP Status Code tương ứng.
- Tuyệt đối không viết business logic, vòng lặp phức tạp hay truy vấn Database trực tiếp trong Controller.

### C. Tầng Ứng dụng (`Application`)
Tổ chức thư mục theo **Feature-driven**:
```text
Features/
└── Auth/
    └── Commands/
        ├── Login/
        │   ├── LoginCommand.cs
        │   └── LoginCommandHandler.cs
        ├── Register/
        │   ├── RegisterUserCommand.cs
        │   └── RegisterUserCommandHandler.cs
        └── VerifyEmail/
            ├── VerifyEmailCommand.cs
            └── VerifyEmailCommandHandler.cs
```

---

## 3. Checklist tự kiểm tra trước khi tạo PR / Commit
1. File này có đang chịu nhiều hơn 1 lý do để thay đổi không? (Vi phạm SRP?)
2. `Program.cs` có dài hơn 50 dòng không?
3. File có đang gom nhiều class/record không liên quan chặt chẽ vào một chỗ không?
4. Unit Tests có được bảo toàn và pass 100% không?

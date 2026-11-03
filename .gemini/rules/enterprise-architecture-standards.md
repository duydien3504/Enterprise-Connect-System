---
description: "Tiêu chuẩn kiến trúc Microservices và an toàn thông tin bắt buộc cho EnterpriseConnect"
globs: ["Services/**", "BuildingBlocks/**", "EnterpriseConnect.ApiGateway/**", "Tests/**"]
---

# Enterprise Architecture & Security Standards

Quy chuẩn này là các bất biến hành vi (Guardrails) bắt buộc áp dụng khi phân tích, thiết kế và lập trình cho toàn bộ các dịch vụ trong hệ thống EnterpriseConnect.

## 1. Database Ownership (Bắt buộc phân tách dữ liệu)
- Mỗi Microservice chỉ sở hữu và kết nối tới Database của riêng mình:
  - `Service 1 (IdentityService)`: `EnterpriseConnect_IdentityDb`
  - `Service 2 (MessagingService)`: `EnterpriseConnect_MessagingDb`
  - `Service 3 (MediaService)`: `EnterpriseConnect_MediaDb`
  - `Service 4 (RemoteService)`: `EnterpriseConnect_RemoteDb`
  - `Service 5 (AdminService)`: `EnterpriseConnect_AdminDb`
- **Cấm tuyệt đối**: Truy cập trực tiếp DB của service khác (cấm cross-db queries, cấm add DbContext chéo giữa các service).

## 2. Giao Tiếp Giữa Các Microservices
- **Giao tiếp Đồng bộ (Synchronous)**:
  - Chỉ sử dụng khi cần dữ liệu hoặc kết quả tức thời (ví dụ: `MessagingService` kiểm tra `CheckBlockStatus` từ `IdentityService`).
  - Sử dụng REST API nội bộ hoặc gRPC.
- **Giao tiếp Bất đồng bộ (Asynchronous Event-Driven)**:
  - Khi có sự kiện làm thay đổi dữ liệu hoặc trạng thái (`UserCreated`, `UserBlocked`, `ContactRequestAccepted`, `MessageSent`, v.v.), bắt buộc phát sự kiện qua RabbitMQ Message Broker.
  - Áp dụng Outbox Pattern, Idempotency và Retry/Dead-Letter Queue để bảo đảm tính nhất quán sau cùng (Eventual Consistency).

## 3. Tiêu Chuẩn Bảo Mật & An Toàn Thông Tin
- **Authentication**:
  - JWT Access Token (ngắn hạn) + Refresh Token (Rotation, lưu `UserSession` với IP, DeviceType, DeviceInfo).
  - Thu hồi toàn bộ session khi đổi mật khẩu, hoặc khi tài khoản bị khóa/banned.
- **Authorization**:
  - Phân quyền theo vai trò (RBAC) kết hợp Permission-based Authorization hạt nhân (`[HasPermission("...")]`).
- **Bảo mật mạng & Chốt chặn**:
  - API Gateway (YARP) làm cổng bảo mật duy nhất cho các kết nối từ bên ngoài tới các microservices.
  - Sử dụng WebRTC kết hợp LiveKit để truyền tải an toàn dữ liệu thoại/video/media.
- **Bảo mật phiên Remote Assistance**:
  - Mã phiên (Session Code) ngẫu nhiên, chỉ dùng 1 lần, có thời hạn.
  - Bắt buộc người dùng phê duyệt quyền (chỉ xem hoặc điều khiển) và cho phép người dùng thu hồi quyền tức thì bất kỳ lúc nào.
- **Audit Logs & Giám sát**:
  - Toàn bộ nhật ký kiểm toán, lịch sử phiên remote, báo cáo vi phạm nội dung phải được lưu trữ tập trung tại `AdminDb`.

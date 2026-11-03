---
name: enterprise-networking
description: "Cẩm nang tra cứu và chuẩn hóa cấu hình mạng, cổng dịch vụ và giao thức truyền thông trong hệ sinh thái EnterpriseConnect theo mô hình OSI 7 tầng"
---

# Enterprise Networking & Protocols Guide

Tài liệu hướng dẫn phân bổ cổng (Port) và giao thức mạng chuẩn hóa trong EnterpriseConnect:

## 1. Bản Đồ Phân Tầng OSI 7 Tầng Trong Hệ Thống

| Tầng OSI | Giao thức / Công nghệ áp dụng | Ứng dụng trong EnterpriseConnect |
| :--- | :--- | :--- |
| **7. Application** | HTTP/HTTPS, REST, gRPC, WebSocket, SignalR, SMTP | API Endpoints, Realtime Chat, File Transfer, Mail OTP |
| **6. Presentation** | TLS 1.3, JSON, JWT, JPEG/WebP | Mã hóa đường truyền HTTPS/WSS, Serialization DTOs, Avatar Re-encoding |
| **5. Session** | WebRTC DataChannels, RPC, Remote Assistance Sessions | Quản lý phiên gọi thoại/video, kết nối điều khiển máy tính từ xa |
| **4. Transport** | TCP (độ tin cậy cao), UDP (tốc độ cao/realtime) | TCP: Database, REST, Redis, RabbitMQ. UDP: WebRTC Voice/Video media |
| **3. Network** | IPv4, IPv6, ICMP | Định địa chỉ client, tracking Client IP trong `UserSession` |
| **2. Data Link** | Ethernet, VLAN | Hạ tầng mạng nội bộ / ảo hóa container |
| **1. Physical** | Fiber, PHY | Hạ tầng server / hosting vật lý |

## 2. Quy Chuẩn Cổng Dịch Vụ Mặc Định (Port Mapping)

- **PostgreSQL**: `5432` (TCP)
- **Redis Cache**: `6379` (TCP)
- **RabbitMQ**: `5672` (AMQP TCP), `15672` (Management UI HTTP)
- **SMTP Server**: `587` (STARTTLS TCP) hoặc `465` (SSL/TLS)
- **YARP API Gateway**: `5000` (HTTP) / `5001` (HTTPS)
- **IdentityService API**: `7063` (HTTPS) / `5063` (HTTP)
- **LiveKit / WebRTC Media Server**: `7880` (HTTP API), `7881` (TCP Signaling), `50000-60000` (UDP Media ICE/STUN/TURN)
- **Remote Assistance Service**: `3389` (RDP chuẩn) hoặc WebSocket Screen Streaming

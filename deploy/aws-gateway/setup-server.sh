#!/usr/bin/env bash
# ==============================================================================
# EnterpriseConnect - EC2 Linux Security Hardening & Gateway Deployment Script
# Chạy trực tiếp bên trong máy chủ Ubuntu 24.04 (qua EC2 Instance Connect Web)
# ==============================================================================

set -euo pipefail

GATEWAY_SECRET="${1:-REDACTED_GATEWAY_SECRET}"
IMAGE_NAME="${2:-enterpriseconnect-gateway:latest}"

echo "=========================================================="
echo "  EnterpriseConnect Server Hardening & Gateway Deploy"
echo "=========================================================="

# 1. Linux Kernel Hardening (CIS Benchmark Defense)
echo ">>> [1/5] Cấu hình Linux Kernel Hardening (/etc/sysctl.d/99-security.conf)..."
sudo bash -c 'cat << "EOF" > /etc/sysctl.d/99-security.conf
# Chống DoS / SYN Flood Attack
net.ipv4.tcp_syncookies = 1
net.ipv4.tcp_max_syn_backlog = 2048
net.ipv4.tcp_synack_retries = 2

# Chống IP Spoofing (Reverse Path Filtering)
net.ipv4.conf.all.rp_filter = 1
net.ipv4.conf.default.rp_filter = 1

# Vô hiệu hóa ICMP Redirects (Chống tấn công Man-in-the-Middle)
net.ipv4.conf.all.accept_redirects = 0
net.ipv4.conf.default.accept_redirects = 0
net.ipv4.conf.all.send_redirects = 0
net.ipv4.conf.default.send_redirects = 0

# Bỏ qua ICMP Broadcast (Chống Smurf Attack)
net.ipv4.icmp_echo_ignore_broadcasts = 1

# Không chấp nhận Source Route packets
net.ipv4.conf.all.accept_source_route = 0
net.ipv4.conf.default.accept_source_route = 0
EOF'

sudo sysctl --system > /dev/null
echo "✓ Đã kích hoạt các quy tắc phòng vệ Kernel!"

# 2. Cấu hình Tường lửa UFW (Host-level Firewall)
echo ">>> [2/5] Thiết lập tường lửa UFW (Default Deny)..."
sudo ufw --force reset > /dev/null
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow 22/tcp comment 'SSH via AWS Instance Connect'
sudo ufw allow 80/tcp comment 'HTTP Web Gateway'
sudo ufw allow 443/tcp comment 'HTTPS Web Gateway'
sudo ufw --force enable
echo "✓ Tường lửa UFW đã kích hoạt thành công!"

# 3. Cài đặt Docker Engine & Docker Compose
echo ">>> [3/5] Kiểm tra và cài đặt Docker Engine..."
if ! command -v docker &> /dev/null; then
  echo "Đang cài đặt Docker chính thức..."
  curl -fsSL https://get.docker.com | sudo sh
  sudo usermod -aG docker "$USER"
fi
sudo systemctl enable --now docker
echo "✓ Docker Engine đã sẵn sàng!"

# 4. Triển khai API Gateway Container
echo ">>> [4/5] Cấu hình và khởi chạy EnterpriseConnect API Gateway..."
sudo mkdir -p /opt/enterpriseconnect/gateway
cd /opt/enterpriseconnect/gateway

# Zero-Leak: Ghi biến môi trường ra file .env với quyền sở hữu 600
sudo bash -c "cat << EOF > .env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_HTTP_PORTS=8080
GATEWAY_SHARED_SECRET=${GATEWAY_SECRET}
Security__GatewaySharedSecret=${GATEWAY_SECRET}
EOF"
sudo chmod 600 .env

sudo bash -c "cat << EOF > docker-compose.yml
services:
  api-gateway:
    image: ${IMAGE_NAME}
    container_name: enterpriseconnect-api-gateway
    restart: unless-stopped
    ports:
      - '80:8080'
    env_file:
      - .env
    healthcheck:
      test: ['CMD-SHELL', 'curl -f http://localhost:8080/health/live || exit 1']
      interval: 15s
      timeout: 5s
      retries: 3
EOF"

echo "Khởi chạy container..."
sudo docker compose down 2>/dev/null || true
sudo docker compose up -d

# 5. Kiểm tra trạng thái
echo ">>> [5/5] Kiểm tra sức khỏe dịch vụ (Health Check)..."
sleep 3

if curl -s http://127.0.0.1/health/live | grep -q "Healthy"; then
  echo ""
  echo "=========================================================="
  echo "  DEPLOY API GATEWAY THÀNH CÔNG RỰC RỠ!"
  echo "=========================================================="
  echo "Health Endpoint : http://localhost/health/live (Trạng thái: Healthy)"
  echo "Shared Secret   : ${GATEWAY_SECRET}"
  echo "Container       : enterpriseconnect-api-gateway (Đang chạy cổng 80)"
  echo "=========================================================="
else
  echo "Cảnh báo: Đang đợi container hoàn tất khởi động. Vui lòng kiểm tra: 'sudo docker compose logs -f'"
fi

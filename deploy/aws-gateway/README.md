# Hướng dẫn Triển khai API Gateway lên AWS EC2 (Chuẩn An ninh & Linux Hardening)

Tài liệu này hướng dẫn chi tiết từng bước đưa `EnterpriseConnect.ApiGateway` lên máy chủ đám mây AWS EC2 tại Region Sydney (`ap-southeast-2`) sử dụng gói $200 credits có sẵn của bạn.

---

## 🚀 Tóm tắt Quy trình 3 Bước

```text
[1. AWS CloudShell] ---> [2. EC2 Instance Connect Web] ---> [3. Truy cập Web Gateway]
   (Chạy 1 lệnh             (Bấm nút Connect trên Web          (Mở http://<IP>/health/live)
  tạo máy chủ EC2)         và chạy script bảo mật)
```

---

## 🛠️ Hướng dẫn Chi tiết

### BƯỚC 1: Khởi tạo Máy chủ trên AWS CloudShell

1. Trên màn hình AWS Console của bạn (như trong ảnh bạn đã gửi), nhìn xuống **góc dưới cùng bên trái**.
2. Bấm vào biểu tượng **`CloudShell`** (terminal trình duyệt).
3. Khi terminal xuất hiện, copy toàn bộ khối lệnh dưới đây và dán (paste) vào CloudShell rồi nhấn **Enter**:

```bash
# Tải script khởi tạo máy chủ EC2
cat << "EOF" > launch-ec2.sh
$(cat << 'INNER_EOF'
#!/usr/bin/env bash
set -euo pipefail
AWS_REGION="${AWS_REGION:-ap-southeast-2}"
INSTANCE_TYPE="t3.small"
SG_NAME="enterpriseconnect-gw-sg"
TAG_NAME="enterpriseconnect-api-gateway"

VPC_ID=$(aws ec2 describe-vpcs --region "$AWS_REGION" --filters "Name=isDefault,Values=true" --query "Vpcs[0].VpcId" --output text)
SG_ID=$(aws ec2 describe-security-groups --region "$AWS_REGION" --filters "Name=group-name,Values=${SG_NAME}" "Name=vpc-id,Values=${VPC_ID}" --query "SecurityGroups[0].GroupId" --output text 2>/dev/null || true)

if [ -z "$SG_ID" ] || [ "$SG_ID" = "None" ]; then
  SG_ID=$(aws ec2 create-security-group --region "$AWS_REGION" --group-name "$SG_NAME" --description "EnterpriseConnect API Gateway Hardened Security Group" --vpc-id "$VPC_ID" --output text --query "GroupId")
  aws ec2 authorize-security-group-ingress --region "$AWS_REGION" --group-id "$SG_ID" --protocol tcp --port 80 --cidr 0.0.0.0/0
  aws ec2 authorize-security-group-ingress --region "$AWS_REGION" --group-id "$SG_ID" --protocol tcp --port 443 --cidr 0.0.0.0/0
  aws ec2 authorize-security-group-ingress --region "$AWS_REGION" --group-id "$SG_ID" --protocol tcp --port 22 --cidr 13.239.158.0/29
fi

AMI_ID=$(aws ssm get-parameter --region "$AWS_REGION" --name "/aws/service/canonical/ubuntu/server/24.04/stable/current/amd64/hvm/ebs-gp3/ami-id" --query "Parameter.Value" --output text 2>/dev/null || true)
if [ -z "$AMI_ID" ] || [ "$AMI_ID" = "None" ]; then
  AMI_ID=$(aws ec2 describe-images --region "$AWS_REGION" --owners 099720109477 --filters "Name=name,Values=ubuntu/images/hvm-ssd-gp3/ubuntu-noble-24.04-amd64-server-*" "Name=state,Values=available" --query "sort_by(Images, &CreationDate)[-1].ImageId" --output text)
fi

EXISTING_ID=$(aws ec2 describe-instances --region "$AWS_REGION" --filters "Name=tag:Name,Values=${TAG_NAME}" "Name=instance-state-name,Values=running,pending" --query "Reservations[0].Instances[0].InstanceId" --output text 2>/dev/null || true)

if [ -n "$EXISTING_ID" ] && [ "$EXISTING_ID" != "None" ]; then
  INSTANCE_ID="$EXISTING_ID"
else
  INSTANCE_ID=$(aws ec2 run-instances --region "$AWS_REGION" --image-id "$AMI_ID" --instance-type "$INSTANCE_TYPE" --security-group-ids "$SG_ID" --block-device-mappings '[{"DeviceName":"/dev/sda1","Ebs":{"VolumeSize":20,"VolumeType":"gp3","DeleteOnTermination":true}}]' --tag-specifications "ResourceType=instance,Tags=[{Key=Name,Value=${TAG_NAME}},{Key=Project,Value=EnterpriseConnect},{Key=Environment,Value=Production}]" --query "Instances[0].InstanceId" --output text)
fi

aws ec2 wait instance-running --region "$AWS_REGION" --instance-ids "$INSTANCE_ID"
PUBLIC_IP=$(aws ec2 describe-instances --region "$AWS_REGION" --instance-ids "$INSTANCE_ID" --query "Reservations[0].Instances[0].PublicIpAddress" --output text)

echo "=========================================================="
echo "KHỞI TẠO THÀNH CÔNG! Public IP của bạn: ${PUBLIC_IP}"
echo "=========================================================="
INNER_EOF
)
EOF

bash launch-ec2.sh
```

4. Đợi khoảng **30 giây**, CloudShell sẽ in ra dòng:  
   `KHỞI TẠO THÀNH CÔNG! Public IP của bạn: <ĐỊA_CHỈ_IP>`

---

### BƯỚC 2: Kết nối vào Máy chủ bằng 1-Click trên Web

1. Trên thanh tìm kiếm trên cùng của AWS Console, gõ **`EC2`** và chọn dịch vụ **EC2**.
2. Ở menu bên trái, bấm vào **Instances**.
3. Bạn sẽ thấy máy chủ tên **`enterpriseconnect-api-gateway`** đang ở trạng thái `Running`.
4. **Tích vào ô vuông** cạnh máy chủ đó, sau đó bấm nút **Connect** ở góc trên bên phải.
5. Giữ nguyên tab **EC2 Instance Connect** (User name: `ubuntu`) và bấm nút **Connect** màu cam ở góc dưới.
6. Một cửa sổ terminal màu đen sẽ mở ra ngay trong trình duyệt của bạn!

---

### BƯỚC 3: Kích hoạt Bảo mật Linux & Chạy Gateway

Trong cửa sổ terminal đen vừa mở ra, bạn dán (paste) lệnh sau rồi nhấn **Enter**:

```bash
# Tải và chạy script Hardening & kích hoạt Docker Gateway
cat << 'EOF' > setup-server.sh
#!/usr/bin/env bash
set -euo pipefail
GATEWAY_SECRET="REDACTED_GATEWAY_SECRET"
IMAGE_NAME="enterpriseconnect-gateway:latest"

echo ">>> [1/4] Linux Kernel Hardening..."
sudo bash -c 'cat << "SYSCTL" > /etc/sysctl.d/99-security.conf
net.ipv4.tcp_syncookies = 1
net.ipv4.conf.all.rp_filter = 1
net.ipv4.conf.default.rp_filter = 1
net.ipv4.conf.all.accept_redirects = 0
net.ipv4.conf.all.send_redirects = 0
net.ipv4.icmp_echo_ignore_broadcasts = 1
net.ipv4.conf.all.accept_source_route = 0
SYSCTL'
sudo sysctl --system > /dev/null

echo ">>> [2/4] Kích hoạt tường lửa UFW..."
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw --force enable

echo ">>> [3/4] Cài đặt Docker..."
if ! command -v docker &> /dev/null; then
  curl -fsSL https://get.docker.com | sudo sh
  sudo usermod -aG docker "$USER"
fi

echo ">>> [4/4] Khởi chạy API Gateway Container..."
sudo mkdir -p /opt/enterpriseconnect/gateway
cd /opt/enterpriseconnect/gateway

sudo bash -c "cat << DOCKEREOF > docker-compose.yml
services:
  api-gateway:
    image: ${IMAGE_NAME}
    container_name: enterpriseconnect-api-gateway
    restart: unless-stopped
    ports:
      - '80:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_HTTP_PORTS=8080
      - GATEWAY_SHARED_SECRET=${GATEWAY_SECRET}
      - Security__GatewaySharedSecret=${GATEWAY_SECRET}
DOCKEREOF"

echo "Setup hoàn tất! Kiểm tra tại http://localhost/health/live"
EOF

chmod +x setup-server.sh
./setup-server.sh
```

---

### BƯỚC 4: Kiểm tra Kết quả Thực tế

Mở một tab mới trên trình duyệt bất kỳ (Chrome / Safari / Firefox) và truy cập:
```text
http://<PUBLIC_IP_CỦA_BẠN>/health/live
```
Bạn sẽ thấy phản hồi JSON màu xanh chuẩn RFC:
```json
{
  "status": "Healthy"
}
```

Chúc mừng bạn đã hoàn thành triển khai máy chủ API Gateway an toàn trên AWS EC2!

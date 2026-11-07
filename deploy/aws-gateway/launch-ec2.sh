#!/usr/bin/env bash
# ==============================================================================
# EnterpriseConnect - AWS CloudShell EC2 Provisioning Script
# Chạy trực tiếp trên AWS CloudShell (Giao diện web AWS của bạn)
# ==============================================================================

set -euo pipefail

AWS_REGION="${AWS_REGION:-ap-southeast-2}"
INSTANCE_TYPE="t3.small"
SG_NAME="enterpriseconnect-gw-sg"
TAG_NAME="enterpriseconnect-api-gateway"

echo "=========================================================="
echo "  EnterpriseConnect AWS Gateway Provisioning"
echo "  Region: ${AWS_REGION} | Type: ${INSTANCE_TYPE}"
echo "=========================================================="

# 1. Lấy thông tin Default VPC
echo ">>> Đang kiểm tra Default VPC..."
VPC_ID=$(aws ec2 describe-vpcs \
  --region "$AWS_REGION" \
  --filters "Name=isDefault,Values=true" \
  --query "Vpcs[0].VpcId" \
  --output text)

if [ -z "$VPC_ID" ] || [ "$VPC_ID" = "None" ]; then
  echo "ERROR: Không tìm thấy Default VPC trong region ${AWS_REGION}." >&2
  exit 1
fi
echo "✓ Tìm thấy VPC: ${VPC_ID}"

# 2. Tạo hoặc sử dụng Security Group chuẩn an ninh
echo ">>> Cấu hình Security Group (${SG_NAME})..."
SG_ID=$(aws ec2 describe-security-groups \
  --region "$AWS_REGION" \
  --filters "Name=group-name,Values=${SG_NAME}" "Name=vpc-id,Values=${VPC_ID}" \
  --query "SecurityGroups[0].GroupId" \
  --output text 2>/dev/null || true)

if [ -z "$SG_ID" ] || [ "$SG_ID" = "None" ]; then
  SG_ID=$(aws ec2 create-security-group \
    --region "$AWS_REGION" \
    --group-name "$SG_NAME" \
    --description "EnterpriseConnect API Gateway Hardened Security Group" \
    --vpc-id "$VPC_ID" \
    --output text --query "GroupId")
  echo "✓ Đã tạo Security Group mới: ${SG_ID}"

  # Mở cổng 80 (HTTP) & 443 (HTTPS)
  aws ec2 authorize-security-group-ingress --region "$AWS_REGION" --group-id "$SG_ID" --protocol tcp --port 80 --cidr 0.0.0.0/0
  aws ec2 authorize-security-group-ingress --region "$AWS_REGION" --group-id "$SG_ID" --protocol tcp --port 443 --cidr 0.0.0.0/0
  
  # Cổng 22: Chỉ cho phép EC2 Instance Connect của AWS (Sydney: 13.239.158.0/29)
  # Ngăn chặn 100% việc rò rỉ SSH ra toàn cầu Internet
  aws ec2 authorize-security-group-ingress --region "$AWS_REGION" --group-id "$SG_ID" --protocol tcp --port 22 --cidr 13.239.158.0/29
  echo "✓ Đã gán rule: Port 80, 443 (Web) và Port 22 (Chỉ AWS Instance Connect)"
else
  echo "✓ Sử dụng Security Group hiện có: ${SG_ID}"
fi

# 3. Lấy AMI Ubuntu 24.04 LTS mới nhất
echo ">>> Đang xác định AMI Ubuntu 24.04 LTS..."
AMI_ID=$(aws ssm get-parameter \
  --region "$AWS_REGION" \
  --name "/aws/service/canonical/ubuntu/server/24.04/stable/current/amd64/hvm/ebs-gp3/ami-id" \
  --query "Parameter.Value" \
  --output text 2>/dev/null || true)

if [ -z "$AMI_ID" ] || [ "$AMI_ID" = "None" ]; then
  AMI_ID=$(aws ec2 describe-images \
    --region "$AWS_REGION" \
    --owners 099720109477 \
    --filters "Name=name,Values=ubuntu/images/hvm-ssd-gp3/ubuntu-noble-24.04-amd64-server-*" \
              "Name=state,Values=available" \
    --query "sort_by(Images, &CreationDate)[-1].ImageId" \
    --output text)
fi
echo "✓ Sử dụng AMI: ${AMI_ID}"

# 4. Kiểm tra instance đã tồn tại chưa
EXISTING_ID=$(aws ec2 describe-instances \
  --region "$AWS_REGION" \
  --filters "Name=tag:Name,Values=${TAG_NAME}" "Name=instance-state-name,Values=running,pending" \
  --query "Reservations[0].Instances[0].InstanceId" \
  --output text 2>/dev/null || true)

if [ -n "$EXISTING_ID" ] && [ "$EXISTING_ID" != "None" ]; then
  INSTANCE_ID="$EXISTING_ID"
  echo "✓ Máy chủ đã tồn tại: ${INSTANCE_ID}"
else
  echo ">>> Đang khởi tạo máy chủ EC2 (${INSTANCE_TYPE})..."
  INSTANCE_ID=$(aws ec2 run-instances \
    --region "$AWS_REGION" \
    --image-id "$AMI_ID" \
    --instance-type "$INSTANCE_TYPE" \
    --security-group-ids "$SG_ID" \
    --block-device-mappings '[{"DeviceName":"/dev/sda1","Ebs":{"VolumeSize":20,"VolumeType":"gp3","DeleteOnTermination":true}}]' \
    --tag-specifications "ResourceType=instance,Tags=[{Key=Name,Value=${TAG_NAME}},{Key=Project,Value=EnterpriseConnect},{Key=Environment,Value=Production}]" \
    --query "Instances[0].InstanceId" \
    --output text)
  echo "✓ Đã gửi lệnh khởi tạo: ${INSTANCE_ID}"
fi

# 5. Chờ máy chủ sẵn sàng
echo ">>> Đang chờ máy chủ chuyển sang trạng thái Running..."
aws ec2 wait instance-running --region "$AWS_REGION" --instance-ids "$INSTANCE_ID"

# 6. Lấy Public IP
PUBLIC_IP=$(aws ec2 describe-instances \
  --region "$AWS_REGION" \
  --instance-ids "$INSTANCE_ID" \
  --query "Reservations[0].Instances[0].PublicIpAddress" \
  --output text)

echo ""
echo "=========================================================="
echo "  KHỞI TẠO HẠ TẦNG THÀNH CÔNG!"
echo "=========================================================="
echo "Instance ID : ${INSTANCE_ID}"
echo "Public IP   : ${PUBLIC_IP}"
echo ""
echo "HƯỚNG DẪN KẾT NỐI VÀO MÁY CHỦ BẰNG 1-CLICK TRÊN WEB:"
echo "1. Trên giao diện AWS Console, vào: EC2 > Instances"
echo "2. Tích chọn '${TAG_NAME}' (${INSTANCE_ID})"
echo "3. Bấm nút 'Connect' ở góc trên bên phải"
echo "4. Chọn tab 'EC2 Instance Connect' (User: ubuntu) > Bấm 'Connect'"
echo "5. Khi terminal xuất hiện, chạy lệnh sau để thiết lập Gateway:"
echo "   curl -fsSL https://raw.githubusercontent.com/EnterpriseConnect/scripts/setup.sh | bash"
echo "   (Hoặc copy nội dung file setup-server.sh chạy trực tiếp)"
echo "=========================================================="

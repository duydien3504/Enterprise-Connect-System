#!/usr/bin/env bash
# ==============================================================================
# Automated Docker Build & Push Script for EnterpriseConnect Microservices (Linux/CI)
# Usage: ./scripts/docker-build-push.sh [REGISTRY] [VERSION] [SERVICES] [PUSH(true|false)]
# ==============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"

REGISTRY="${1:-${DOCKER_REGISTRY_NAMESPACE:-}}"
VERSION="${2:-1.0.0}"
SERVICES="${3:-all}"
PUSH="${4:-false}"

echo "=========================================================="
echo "  EnterpriseConnect Docker Build & Push Pipeline (Bash)"
echo "=========================================================="
echo "Repo Root: ${REPO_ROOT}"
echo "Registry : ${REGISTRY:-"(local only)"}"
echo "Version  : ${VERSION}"
echo "Push     : ${PUSH}"
echo "Services : ${SERVICES}"
echo "----------------------------------------------------------"

# 1. Verify Docker Daemon
if ! docker info >/dev/null 2>&1; then
    echo "ERROR: Docker daemon is not running." >&2
    exit 1
fi

# 2. Authenticate if push requested
if [ "$PUSH" = "true" ]; then
    if [ -z "$REGISTRY" ]; then
        echo "ERROR: Registry is required when PUSH is true." >&2
        exit 1
    fi
    if [ -n "${DOCKER_USERNAME:-}" ] && [ -n "${DOCKER_PASSWORD:-${DOCKER_PAT:-}}" ]; then
        echo "Logging into Docker Hub..."
        PASS="${DOCKER_PAT:-$DOCKER_PASSWORD}"
        echo "$PASS" | docker login -u "$DOCKER_USERNAME" --password-stdin
    fi
fi

# 3. Services Definition (key:name:dockerfile)
SERVICE_LIST=(
    "gateway:enterpriseconnect-gateway:EnterpriseConnect.ApiGateway/Dockerfile"
    "identity:enterpriseconnect-identity:Services/IdentityService/EnterpriseConnect.IdentityService.Api/Dockerfile"
    "messaging:enterpriseconnect-messaging:Services/MessagingService/EnterpriseConnect.MessagingService.Api/Dockerfile"
    "media:enterpriseconnect-media:Services/MediaService/EnterpriseConnect.MediaService.Api/Dockerfile"
    "remote:enterpriseconnect-remote:Services/RemoteService/EnterpriseConnect.RemoteService.Api/Dockerfile"
    "admin:enterpriseconnect-admin:Services/AdminService/EnterpriseConnect.AdminService.Api/Dockerfile"
)

# 4. Build and Push Loop
for item in "${SERVICE_LIST[@]}"; do
    IFS=":" read -r key name dockerfile <<< "$item"

    if [ "$SERVICES" != "all" ] && [[ ! ",$SERVICES," =~ ",$key," ]]; then
        continue
    fi

    if [ -n "$REGISTRY" ]; then
        VERSION_TAG="${REGISTRY}/${name}:${VERSION}"
        LATEST_TAG="${REGISTRY}/${name}:latest"
    else
        VERSION_TAG="${name}:${VERSION}"
        LATEST_TAG="${name}:latest"
    fi

    echo ""
    echo ">>> Building ${name}..."
    docker build -f "${REPO_ROOT}/${dockerfile}" \
        -t "${VERSION_TAG}" \
        -t "${LATEST_TAG}" \
        "${REPO_ROOT}"

    if [ "$PUSH" = "true" ]; then
        echo ">>> Pushing ${name} to registry..."
        docker push "${VERSION_TAG}"
        docker push "${LATEST_TAG}"
    fi
done

echo ""
echo "=========================================================="
echo "  All requested microservices processed successfully!"
echo "=========================================================="

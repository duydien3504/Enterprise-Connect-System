<#
.SYNOPSIS
    Automated Docker Multi-Stage Build & Push Script for EnterpriseConnect Microservices.
.PARAMETER Registry
    Docker Hub username or organization namespace (e.g. 'mycompany').
.PARAMETER Version
    Image version tag (e.g. '1.0.0' or git commit sha). Defaults to '1.0.0'.
.PARAMETER Services
    Target services: 'all' or comma-separated list ('gateway,identity,messaging,media,remote,admin').
.PARAMETER Push
    Switch flag to push built images to Private Docker Hub.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$Registry = $env:DOCKER_REGISTRY_NAMESPACE,

    [Parameter(Mandatory = $false)]
    [string]$Version = "1.0.0",

    [Parameter(Mandatory = $false)]
    [string]$Services = "all",

    [Parameter(Mandatory = $false)]
    [switch]$Push
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  EnterpriseConnect Docker Build & Push Pipeline" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "Repo Root: $RepoRoot"
Write-Host "Registry : $(if ($Registry) { $Registry } else { '(local only)' })"
Write-Host "Version  : $Version"
Write-Host "Push     : $Push"
Write-Host "Services : $Services"
Write-Host "----------------------------------------------------------"

# 1. Verify Docker Daemon
try {
    docker info > $null 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Docker daemon is not running. Please start Docker Desktop/Engine."
    }
} catch {
    Write-Error "Docker daemon check failed: $_"
    exit 1
}

# 2. Authenticate if push requested
if ($Push) {
    if (-not $Registry) {
        Write-Error "Registry parameter (-Registry) or DOCKER_REGISTRY_NAMESPACE env var is required when -Push is enabled."
        exit 1
    }
    if ($env:DOCKER_USERNAME -and ($env:DOCKER_PASSWORD -or $env:DOCKER_PAT)) {
        Write-Host "Logging into Docker Hub..." -ForegroundColor Yellow
        $token = if ($env:DOCKER_PAT) { $env:DOCKER_PAT } else { $env:DOCKER_PASSWORD }
        $token | docker login -u $env:DOCKER_USERNAME --password-stdin
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Docker login failed. Check credentials."
            exit 1
        }
    }
}

# 3. Microservices Definition
$serviceMap = [ordered]@{
    "gateway"   = @{ Name = "enterpriseconnect-gateway"; Dockerfile = "EnterpriseConnect.ApiGateway/Dockerfile" }
    "identity"  = @{ Name = "enterpriseconnect-identity"; Dockerfile = "Services/IdentityService/EnterpriseConnect.IdentityService.Api/Dockerfile" }
    "messaging" = @{ Name = "enterpriseconnect-messaging"; Dockerfile = "Services/MessagingService/EnterpriseConnect.MessagingService.Api/Dockerfile" }
    "media"     = @{ Name = "enterpriseconnect-media"; Dockerfile = "Services/MediaService/EnterpriseConnect.MediaService.Api/Dockerfile" }
    "remote"    = @{ Name = "enterpriseconnect-remote"; Dockerfile = "Services/RemoteService/EnterpriseConnect.RemoteService.Api/Dockerfile" }
    "admin"     = @{ Name = "enterpriseconnect-admin"; Dockerfile = "Services/AdminService/EnterpriseConnect.AdminService.Api/Dockerfile" }
}

$selectedKeys = if ($Services -eq "all") {
    $serviceMap.Keys
} else {
    $Services.Split(",") | ForEach-Object { $_.Trim().ToLower() }
}

$summary = @()

# 4. Build and Push Loop
foreach ($key in $selectedKeys) {
    if (-not $serviceMap.Contains($key)) {
        Write-Warning "Service '$key' not recognized. Skipping."
        continue
    }

    $svc = $serviceMap[$key]
    $imageName = $svc.Name
    $dockerfilePath = Join-Path $RepoRoot $svc.Dockerfile

    $versionTag = if ($Registry) { "${Registry}/${imageName}:${Version}" } else { "${imageName}:${Version}" }
    $latestTag = if ($Registry) { "${Registry}/${imageName}:latest" } else { "${imageName}:latest" }

    Write-Host "`n>>> Building $imageName..." -ForegroundColor Green
    Write-Host "Dockerfile: $dockerfilePath"
    Write-Host "Tags: $versionTag | $latestTag"

    $buildCmd = "docker build -f `"$dockerfilePath`" -t `"$versionTag`" -t `"$latestTag`" `"$RepoRoot`""
    Invoke-Expression $buildCmd
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build image $imageName"
        exit 1
    }

    $pushStatus = "N/A"
    if ($Push) {
        Write-Host ">>> Pushing $imageName to registry..." -ForegroundColor Yellow
        docker push $versionTag
        if ($LASTEXITCODE -ne 0) { throw "Failed to push $versionTag" }
        docker push $latestTag
        if ($LASTEXITCODE -ne 0) { throw "Failed to push $latestTag" }
        $pushStatus = "Pushed"
    }

    $summary += [PSCustomObject]@{
        Service = $key
        Image   = $imageName
        Version = $Version
        Status  = "Built"
        Push    = $pushStatus
    }
}

# 5. Output Summary
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "  Pipeline Summary" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
$summary | Format-Table -AutoSize
Write-Host "All tasks completed successfully!" -ForegroundColor Green

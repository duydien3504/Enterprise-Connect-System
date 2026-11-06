FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy BuildingBlocks csproj files for layer caching
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Domain/EnterpriseConnect.BuildingBlocks.Domain.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Domain/"]
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Application/EnterpriseConnect.BuildingBlocks.Application.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Application/"]
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Infrastructure/EnterpriseConnect.BuildingBlocks.Infrastructure.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Infrastructure/"]

# Copy IdentityService csproj files
COPY ["Services/IdentityService/EnterpriseConnect.IdentityService.Domain/EnterpriseConnect.IdentityService.Domain.csproj", "Services/IdentityService/EnterpriseConnect.IdentityService.Domain/"]
COPY ["Services/IdentityService/EnterpriseConnect.IdentityService.Application/EnterpriseConnect.IdentityService.Application.csproj", "Services/IdentityService/EnterpriseConnect.IdentityService.Application/"]
COPY ["Services/IdentityService/EnterpriseConnect.IdentityService.Infrastructure/EnterpriseConnect.IdentityService.Infrastructure.csproj", "Services/IdentityService/EnterpriseConnect.IdentityService.Infrastructure/"]
COPY ["Services/IdentityService/EnterpriseConnect.IdentityService.Api/EnterpriseConnect.IdentityService.Api.csproj", "Services/IdentityService/EnterpriseConnect.IdentityService.Api/"]

# Restore only IdentityService.Api and its dependencies
RUN dotnet restore "Services/IdentityService/EnterpriseConnect.IdentityService.Api/EnterpriseConnect.IdentityService.Api.csproj"

# Copy source code
COPY BuildingBlocks/ BuildingBlocks/
COPY Services/IdentityService/ Services/IdentityService/

# Publish
WORKDIR "/src/Services/IdentityService/EnterpriseConnect.IdentityService.Api"
RUN dotnet publish "EnterpriseConnect.IdentityService.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage: Chiseled Ubuntu .NET 8 Distroless
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EnterpriseConnect.IdentityService.Api.dll"]

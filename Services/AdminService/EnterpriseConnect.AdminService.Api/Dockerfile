FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy BuildingBlocks csproj files for layer caching
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Domain/EnterpriseConnect.BuildingBlocks.Domain.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Domain/"]
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Application/EnterpriseConnect.BuildingBlocks.Application.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Application/"]
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Infrastructure/EnterpriseConnect.BuildingBlocks.Infrastructure.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Infrastructure/"]

# Copy AdminService csproj files
COPY ["Services/AdminService/EnterpriseConnect.AdminService.Domain/EnterpriseConnect.AdminService.Domain.csproj", "Services/AdminService/EnterpriseConnect.AdminService.Domain/"]
COPY ["Services/AdminService/EnterpriseConnect.AdminService.Application/EnterpriseConnect.AdminService.Application.csproj", "Services/AdminService/EnterpriseConnect.AdminService.Application/"]
COPY ["Services/AdminService/EnterpriseConnect.AdminService.Infrastructure/EnterpriseConnect.AdminService.Infrastructure.csproj", "Services/AdminService/EnterpriseConnect.AdminService.Infrastructure/"]
COPY ["Services/AdminService/EnterpriseConnect.AdminService.Api/EnterpriseConnect.AdminService.Api.csproj", "Services/AdminService/EnterpriseConnect.AdminService.Api/"]

# Restore only AdminService.Api and its dependencies
RUN dotnet restore "Services/AdminService/EnterpriseConnect.AdminService.Api/EnterpriseConnect.AdminService.Api.csproj"

# Copy source code
COPY BuildingBlocks/ BuildingBlocks/
COPY Services/AdminService/ Services/AdminService/

# Publish
WORKDIR "/src/Services/AdminService/EnterpriseConnect.AdminService.Api"
RUN dotnet publish "EnterpriseConnect.AdminService.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage: Chiseled Ubuntu .NET 8 Distroless
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EnterpriseConnect.AdminService.Api.dll"]

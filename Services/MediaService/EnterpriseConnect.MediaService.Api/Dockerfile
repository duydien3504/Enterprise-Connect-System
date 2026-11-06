FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy BuildingBlocks csproj files for layer caching
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Domain/EnterpriseConnect.BuildingBlocks.Domain.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Domain/"]
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Application/EnterpriseConnect.BuildingBlocks.Application.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Application/"]
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Infrastructure/EnterpriseConnect.BuildingBlocks.Infrastructure.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Infrastructure/"]

# Copy MediaService csproj files
COPY ["Services/MediaService/EnterpriseConnect.MediaService.Domain/EnterpriseConnect.MediaService.Domain.csproj", "Services/MediaService/EnterpriseConnect.MediaService.Domain/"]
COPY ["Services/MediaService/EnterpriseConnect.MediaService.Application/EnterpriseConnect.MediaService.Application.csproj", "Services/MediaService/EnterpriseConnect.MediaService.Application/"]
COPY ["Services/MediaService/EnterpriseConnect.MediaService.Infrastructure/EnterpriseConnect.MediaService.Infrastructure.csproj", "Services/MediaService/EnterpriseConnect.MediaService.Infrastructure/"]
COPY ["Services/MediaService/EnterpriseConnect.MediaService.Api/EnterpriseConnect.MediaService.Api.csproj", "Services/MediaService/EnterpriseConnect.MediaService.Api/"]

# Restore only MediaService.Api and its dependencies
RUN dotnet restore "Services/MediaService/EnterpriseConnect.MediaService.Api/EnterpriseConnect.MediaService.Api.csproj"

# Copy source code
COPY BuildingBlocks/ BuildingBlocks/
COPY Services/MediaService/ Services/MediaService/

# Publish
WORKDIR "/src/Services/MediaService/EnterpriseConnect.MediaService.Api"
RUN dotnet publish "EnterpriseConnect.MediaService.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage: Chiseled Ubuntu .NET 8 Distroless
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EnterpriseConnect.MediaService.Api.dll"]

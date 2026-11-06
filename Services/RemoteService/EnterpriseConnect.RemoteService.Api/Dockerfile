FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy BuildingBlocks csproj files for layer caching
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Domain/EnterpriseConnect.BuildingBlocks.Domain.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Domain/"]
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Application/EnterpriseConnect.BuildingBlocks.Application.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Application/"]
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Infrastructure/EnterpriseConnect.BuildingBlocks.Infrastructure.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Infrastructure/"]

# Copy RemoteService csproj files
COPY ["Services/RemoteService/EnterpriseConnect.RemoteService.Domain/EnterpriseConnect.RemoteService.Domain.csproj", "Services/RemoteService/EnterpriseConnect.RemoteService.Domain/"]
COPY ["Services/RemoteService/EnterpriseConnect.RemoteService.Application/EnterpriseConnect.RemoteService.Application.csproj", "Services/RemoteService/EnterpriseConnect.RemoteService.Application/"]
COPY ["Services/RemoteService/EnterpriseConnect.RemoteService.Infrastructure/EnterpriseConnect.RemoteService.Infrastructure.csproj", "Services/RemoteService/EnterpriseConnect.RemoteService.Infrastructure/"]
COPY ["Services/RemoteService/EnterpriseConnect.RemoteService.Api/EnterpriseConnect.RemoteService.Api.csproj", "Services/RemoteService/EnterpriseConnect.RemoteService.Api/"]

# Restore only RemoteService.Api and its dependencies
RUN dotnet restore "Services/RemoteService/EnterpriseConnect.RemoteService.Api/EnterpriseConnect.RemoteService.Api.csproj"

# Copy source code
COPY BuildingBlocks/ BuildingBlocks/
COPY Services/RemoteService/ Services/RemoteService/

# Publish
WORKDIR "/src/Services/RemoteService/EnterpriseConnect.RemoteService.Api"
RUN dotnet publish "EnterpriseConnect.RemoteService.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage: Chiseled Ubuntu .NET 8 Distroless
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EnterpriseConnect.RemoteService.Api.dll"]

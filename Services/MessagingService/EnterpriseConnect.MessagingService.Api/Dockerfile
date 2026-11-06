FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy BuildingBlocks csproj files for layer caching
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Domain/EnterpriseConnect.BuildingBlocks.Domain.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Domain/"]
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Application/EnterpriseConnect.BuildingBlocks.Application.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Application/"]
COPY ["BuildingBlocks/EnterpriseConnect.BuildingBlocks.Infrastructure/EnterpriseConnect.BuildingBlocks.Infrastructure.csproj", "BuildingBlocks/EnterpriseConnect.BuildingBlocks.Infrastructure/"]

# Copy MessagingService csproj files
COPY ["Services/MessagingService/EnterpriseConnect.MessagingService.Domain/EnterpriseConnect.MessagingService.Domain.csproj", "Services/MessagingService/EnterpriseConnect.MessagingService.Domain/"]
COPY ["Services/MessagingService/EnterpriseConnect.MessagingService.Application/EnterpriseConnect.MessagingService.Application.csproj", "Services/MessagingService/EnterpriseConnect.MessagingService.Application/"]
COPY ["Services/MessagingService/EnterpriseConnect.MessagingService.Infrastructure/EnterpriseConnect.MessagingService.Infrastructure.csproj", "Services/MessagingService/EnterpriseConnect.MessagingService.Infrastructure/"]
COPY ["Services/MessagingService/EnterpriseConnect.MessagingService.Api/EnterpriseConnect.MessagingService.Api.csproj", "Services/MessagingService/EnterpriseConnect.MessagingService.Api/"]

# Restore only MessagingService.Api and its dependencies
RUN dotnet restore "Services/MessagingService/EnterpriseConnect.MessagingService.Api/EnterpriseConnect.MessagingService.Api.csproj"

# Copy source code
COPY BuildingBlocks/ BuildingBlocks/
COPY Services/MessagingService/ Services/MessagingService/

# Publish
WORKDIR "/src/Services/MessagingService/EnterpriseConnect.MessagingService.Api"
RUN dotnet publish "EnterpriseConnect.MessagingService.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage: Chiseled Ubuntu .NET 8 Distroless
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EnterpriseConnect.MessagingService.Api.dll"]

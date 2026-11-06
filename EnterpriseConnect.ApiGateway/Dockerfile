FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file for caching
COPY ["EnterpriseConnect.ApiGateway/EnterpriseConnect.ApiGateway.csproj", "EnterpriseConnect.ApiGateway/"]
RUN dotnet restore "EnterpriseConnect.ApiGateway/EnterpriseConnect.ApiGateway.csproj"

# Copy source code and publish
COPY EnterpriseConnect.ApiGateway/ EnterpriseConnect.ApiGateway/
WORKDIR "/src/EnterpriseConnect.ApiGateway"
RUN dotnet publish "EnterpriseConnect.ApiGateway.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage: Chiseled Ubuntu .NET 8 Distroless
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EnterpriseConnect.ApiGateway.dll"]

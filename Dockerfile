# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/VaultCore.API/VaultCore.API.csproj", "VaultCore.API/"]
COPY ["src/VaultCore.Infrastructure/VaultCore.Infrastructure.csproj", "VaultCore.Infrastructure/"]
COPY ["src/VaultCore.Application/VaultCore.Application.csproj", "VaultCore.Application/"]
COPY ["src/VaultCore.Domain/VaultCore.Domain.csproj", "VaultCore.Domain/"]
RUN dotnet restore "VaultCore.API/VaultCore.API.csproj"
COPY src/ .
WORKDIR "/src/VaultCore.API"
RUN dotnet build "VaultCore.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "VaultCore.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VaultCore.API.dll"]

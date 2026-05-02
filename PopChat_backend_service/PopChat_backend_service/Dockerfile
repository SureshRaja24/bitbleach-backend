# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copy everything (This brings in the 'Backend' folder)
COPY . .

# 2. Move to the directory where the Solution/Project lives
# Path: /src/Backend/PopChat_backend_service
WORKDIR "/src/Backend/PopChat_backend_service"

# 3. Restore using the .sln (found here)
RUN dotnet restore

# 4. Publish the specific project
# Path to .csproj from here: PopChat_backend_service/PopChat_backend_service.csproj
RUN dotnet publish "PopChat_backend_service/PopChat_backend_service.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Render Config
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

# Start the DLL
ENTRYPOINT ["dotnet", "PopChat_backend_service.dll"]

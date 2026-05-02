# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copy everything
COPY . .

# 2. Automatically find the directory containing a .csproj or .sln and move there
# This handles any case-sensitivity (backend vs Backend)
RUN cd $(find . -name "*.csproj" -exec dirname {} \;) && dotnet restore

# 3. Publish from the directory where the .csproj lives
RUN cd $(find . -name "*.csproj" -exec dirname {} \;) && dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

# Start the application
ENTRYPOINT ["dotnet", "PopChat_backend_service.dll"]

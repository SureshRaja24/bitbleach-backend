# Stage 1: Build using the .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything from the repo
COPY . .

# Find the .csproj anywhere in the Backend folder and restore
# Using 'find' ensures we hit the nested folder correctly
RUN dotnet restore $(find Backend -name "*.csproj")

# Publish the project 
RUN dotnet publish $(find Backend -name "*.csproj") -c Release -o /app/publish

# Stage 2: Runtime using .NET 10 ASP.NET image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Render Specifics
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

# This must match your assembly name
ENTRYPOINT ["dotnet", "PopChat_backend_service.dll"]

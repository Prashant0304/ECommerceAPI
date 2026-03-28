# Stage 1: Build using .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# This wildcard (*) will find "ECommerceAPI.csproj" regardless of case!
COPY *.csproj ./
RUN dotnet restore

# Copy all other files (Controllers, Models, etc.)
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime using .NET 10 ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# IMPORTANT: This must match the name in your 'dir' list + .dll
ENTRYPOINT ["dotnet", "ECommerceAPI.dll"]
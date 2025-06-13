# Base image untuk runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy .csproj dan restore dependencies
COPY API_DikaWaroong/API_DikaWaroong.csproj API_DikaWaroong/
RUN dotnet restore API_DikaWaroong/API_DikaWaroong.csproj

# Copy semua isi dan build project
COPY . .
WORKDIR /src/API_DikaWaroong
RUN dotnet publish -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API_DikaWaroong.dll"]

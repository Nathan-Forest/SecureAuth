# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["SecureAuthAPI.csproj", "./"]
RUN dotnet restore "SecureAuthAPI.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "SecureAuthAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SecureAuthAPI.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "SecureAuthAPI.dll"]
# ==========================================
# Stage 1: Build and Publish
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies first.
# This allows Docker to cache the restore layer if your dependencies haven't changed.
COPY ["SportAPI.csproj", "./"]
RUN dotnet restore "SportAPI.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish the application
# /p:UseAppHost=false prevents generating a native executable, saving space in the container
RUN dotnet publish "SportAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# Stage 2: Final Runtime Image
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Expose port 8080 (the default port for .NET 8+ containers)
EXPOSE 8080

# Switch to the built-in non-root user for enhanced security
USER $APP_UID

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Set the entry point to run your application
ENTRYPOINT ["dotnet", "SportAPI.dll"]
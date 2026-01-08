# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy solution and project files
COPY UniShare.sln .
COPY UniShare/UniShare.csproj ./UniShare/
COPY tests/UniShare.Tests/UniShare.Tests.csproj ./tests/UniShare.Tests/

# Restore dependencies
RUN dotnet restore UniShare.sln

# Copy the rest of the source code
COPY . .

# Publish the application
WORKDIR /source/UniShare
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "UniShare.dll"]
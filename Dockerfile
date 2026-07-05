# Stage 1 — build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY src/TechWorld.Domain/TechWorld.Domain.csproj           src/TechWorld.Domain/
COPY src/TechWorld.Application/TechWorld.Application.csproj src/TechWorld.Application/
COPY src/TechWorld.Infrastructure/TechWorld.Infrastructure.csproj src/TechWorld.Infrastructure/
COPY src/TechWorld.API/TechWorld.API.csproj                 src/TechWorld.API/

RUN dotnet restore src/TechWorld.API/TechWorld.API.csproj

COPY . .

RUN dotnet publish src/TechWorld.API/TechWorld.API.csproj \
    -c Release -o /app/publish --no-restore

# Stage 2 — runtime (non-root user, no env baked in)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

# Run as non-root (the aspnet image ships with an 'app' user)
USER app

EXPOSE 8080
ENTRYPOINT ["dotnet", "TechWorld.API.dll"]

# =========================
# STAGE 1 — RESTORE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copio il csproj prima per sfruttare la cache Docker
COPY KlodTattooWeb.csproj ./
RUN dotnet restore

# Copio tutto il resto del progetto
COPY . .

# Build in modalità Release
RUN dotnet build -c Release -o /app/build

# =========================
# STAGE 2 — PUBLISH
# =========================
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# =========================
# STAGE 3 — RUNTIME
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Installa curl per health check (opzionale ma utile)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copia l'app pubblicata
COPY --from=publish /app/publish .

# Railway passerà la variabile PORT a runtime
# Non serve settarla qui, il Program.cs la leggerà
ENV ASPNETCORE_ENVIRONMENT=Production

# Porta di default (Railway la sovrascriverà)
EXPOSE 8080

# Health check per Railway (verifica che l'app risponda)
HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "KlodTattooWeb.dll"]
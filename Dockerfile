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

# Copio l'app pubblicata
COPY --from=publish /app/publish .

# Variabile PORT richiesta da Railway
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Porta esposta (non obbligatoria ma consigliata)
EXPOSE 8080

ENTRYPOINT ["dotnet", "KlodTattooWeb.dll"]

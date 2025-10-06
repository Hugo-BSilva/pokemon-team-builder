# ==============================
# 🧱 STAGE 1 — BUILD
# ==============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia apenas o csproj (cache otimizado)
COPY pokemon-team-builder.csproj ./
RUN dotnet restore

# Copia o restante do código-fonte
COPY . .

# Publica com otimizações para produção
RUN dotnet publish -c Release -o /app/publish \
    -p:PublishReadyToRun=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=partial \
    -p:InvariantGlobalization=true

# ==============================
# 🚀 STAGE 2 — RUNTIME
# ==============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0-slim AS runtime
WORKDIR /app

# Configurações de ambiente padrão para Render
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Copia build publicado da stage anterior
COPY --from=build /app/publish ./

# Expõe a porta usada pelo Render
EXPOSE 8080

# Inicia a aplicação
ENTRYPOINT ["dotnet", "pokemon-team-builder.dll"]
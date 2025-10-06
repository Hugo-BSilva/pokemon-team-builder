# ---------- STAGE 1: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia apenas o csproj e restaura dependências (cache eficiente)
COPY pokemon-team-builder.csproj ./
RUN dotnet restore

# Copia o resto do código
COPY . .

# Publica com ReadyToRun e trimming para startup mais rápido e menor tamanho
RUN dotnet publish -c Release -o /app/publish \
    -p:PublishReadyToRun=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=partial \
    -p:InvariantGlobalization=true

# ---------- STAGE 2: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Define variáveis de ambiente recomendadas
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
    ASPNETCORE_ENVIRONMENT=Production

# Copia build otimizado
COPY --from=build /app/publish ./

# Usa dotnet diretamente
ENTRYPOINT ["dotnet", "pokemon-team-builder.dll"]

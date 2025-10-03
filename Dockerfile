# Estágio 1: Build (Construção)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY pokemon-team-builder.csproj .
# Removemos a restauração do build anterior. Vamos usar o restore implícito no publish.

# 2. Copia todo o restante do código-fonte
COPY . .

# 3. Publica a aplicação
RUN dotnet publish -c Release -o /app/publish

# Estágio 2: Runtime (Execução)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Adiciona a variável de ambiente para forçar a escuta na porta 8080 (padrão do Docker)
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "pokemon-team-builder.dll"]
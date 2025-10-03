# Estágio 1: Build (Construção)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# 1. Copia apenas o arquivo de projeto (csproj) para o contêiner
# O Docker o vê na raiz (o '.') e copia para a raiz do WORKDIR (/app).
COPY pokemon-team-builder.csproj .
RUN dotnet restore

# 2. Copia todo o restante do código-fonte (Program.cs, Controllers, etc.)
# O Docker o vê na raiz (o '.') e copia para a raiz do WORKDIR (/app).
COPY . .

# 3. Publica a aplicação
# O comando agora assume que você está no diretório correto (/app)
RUN dotnet publish -c Release -o /app/publish --no-restore

# Estágio 2: Runtime (Execução)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Define o comando de inicialização da API
ENTRYPOINT ["dotnet", "pokemon-team-builder.dll"]
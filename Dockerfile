# Estágio 1: Build (Construção)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# 1. Copia apenas o arquivo de projeto (csproj) e restaura
COPY pokemon-team-builder.csproj .
# O .sln é copiado no próximo passo, mas ele está na raiz.
# Removemos o restore separado, e confiamos no Publish para restaurar e construir.

# 2. Copia todo o restante do código-fonte
# Adicionamos um .dockerignore (próximo passo) para ignorar o bin/obj
COPY . .

# 3. Publica a aplicação.
# O parâmetro --no-restore CAUSOU o erro, pois o publish não conseguiu resolver dependências.
# Iremos REMOVER o --no-restore para forçar a restauração dentro do publish.
# O /t:PublishSingleFile é adicionado para garantir que ele publique APENAS o projeto.
RUN dotnet publish -c Release -o /app/publish

# Estágio 2: Runtime (Execução)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "pokemon-team-builder.dll"]
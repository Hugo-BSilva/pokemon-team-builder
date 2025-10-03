# 1. ESTÁGIO DE BUILD
# Usa a imagem oficial do .NET SDK para compilar o código.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo de projeto e restaura as dependências
COPY ["pokemon-team-builder/pokemon-team-builder.csproj", "pokemon-team-builder/"]
RUN dotnet restore "pokemon-team-builder/pokemon-team-builder.csproj"

# Copia todo o código-fonte restante
COPY . .
WORKDIR "/src/pokemon-team-builder"

# Publica a aplicação em modo Release
RUN dotnet publish "pokemon-team-builder.csproj" -c Release -o /app/publish --no-restore

# 2. ESTÁGIO DE EXECUÇÃO
# Usa uma imagem runtime mais leve e segura (sem o SDK)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Define o ponto de entrada (Start Command)
# O nome da DLL é o nome do projeto (pokemon-team-builder.dll)
ENTRYPOINT ["dotnet", "pokemon-team-builder.dll"]
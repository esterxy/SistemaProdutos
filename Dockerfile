# ==========================================
# Estágio de Compilação (Build Stage)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj e restaurar dependências (otimização de cache de camadas)
COPY ["Api/SistemaProdutos.csproj", "Api/"]
RUN dotnet restore "Api/SistemaProdutos.csproj"

# Copiar todos os arquivos restantes da aplicação
COPY . .

# Mudar para o diretório do projeto e publicar os binários
WORKDIR "/src/Api"
RUN dotnet publish "SistemaProdutos.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# Estágio de Execução (Runtime Stage)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copiar a saída compilada do estágio de build
COPY --from=build /app/publish .

# Variáveis de ambiente para o contêiner
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Expor porta padrão de documentação (8080 é o padrão do .NET 8/9/10 para contêineres não-root)
EXPOSE 8080

# Iniciar a API (.NET carrega o wwwroot e hospeda a SPA dinamicamente)
ENTRYPOINT ["dotnet", "SistemaProdutos.dll"]

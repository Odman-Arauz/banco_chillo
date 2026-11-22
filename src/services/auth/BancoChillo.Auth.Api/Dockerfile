# Imagen Docker de BancoChillo.Auth.Api — se construye desde la RAÍZ del repositorio:
#   docker build -f src/services/auth/BancoChillo.Auth.Api/Dockerfile -t banco-chillo/auth .

# ---------- Etapa 1: compilar ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /fuente
COPY . .
RUN dotnet publish src/services/auth/BancoChillo.Auth.Api/BancoChillo.Auth.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Etapa 2: imagen final liviana (solo runtime, sin SDK ni código fuente) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Usuario sin privilegios incluido en la imagen oficial de .NET.
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "BancoChillo.Auth.Api.dll"]

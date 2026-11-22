# Imagen Docker de BancoChillo.Auditoria.Api — se construye desde la RAÍZ del repositorio:
#   docker build -f src/services/auditoria/BancoChillo.Auditoria.Api/Dockerfile -t banco-chillo/auditoria .

# ---------- Etapa 1: compilar ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /fuente
COPY . .
RUN dotnet publish src/services/auditoria/BancoChillo.Auditoria.Api/BancoChillo.Auditoria.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Etapa 2: imagen final liviana (solo runtime, sin SDK ni código fuente) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Usuario sin privilegios incluido en la imagen oficial de .NET.
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "BancoChillo.Auditoria.Api.dll"]

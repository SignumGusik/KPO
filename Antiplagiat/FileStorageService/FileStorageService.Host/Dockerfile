# ====== 1. Базовый образ с ASP.NET (runtime) ======
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5020

# ====== 2. Образ для сборки (SDK) ======
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj-файлы
COPY FileStorageService.Entities/FileStorageService.Entities.csproj FileStorageService.Entities/
COPY FileStorageService.UseCases/FileStorageService.UseCases.csproj FileStorageService.UseCases/
COPY FileStorageService.Infrastructure/FileStorageService.Infrastructure.csproj FileStorageService.Infrastructure/
COPY FileStorageService.Presentation/FileStorageService.Presentation.csproj FileStorageService.Presentation/
COPY FileStorageService.Host/FileStorageService.Host.csproj FileStorageService.Host/

# восстановление пакетов
RUN dotnet restore "FileStorageService.Host/FileStorageService.Host.csproj"

# копируем всё решение в образ
COPY . .

# публикуем
RUN dotnet publish "FileStorageService.Host/FileStorageService.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ====== 3. Финальный тонкий образ ======
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
# слушаем на всех интерфейсах (порт зададим из docker-compose)
ENV ASPNETCORE_URLS=http://+:5020
ENTRYPOINT ["dotnet", "FileStorageService.Host.dll"]
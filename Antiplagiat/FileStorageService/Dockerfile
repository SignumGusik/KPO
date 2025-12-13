
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FileStorageService.Entities/FileStorageService.Entities.csproj", "FileStorageService.Entities/"]
COPY ["FileStorageService.UseCases/FileStorageService.UseCases.csproj", "FileStorageService.UseCases/"]
COPY ["FileStorageService.Infrastructure/FileStorageService.Infrastructure.csproj", "FileStorageService.Infrastructure/"]
COPY ["FileStorageService.Presentation/FileStorageService.Presentation.csproj", "FileStorageService.Presentation/"]
COPY ["FileStorageService.Host/FileStorageService.Host.csproj", "FileStorageService.Host/"]

RUN dotnet restore "FileStorageService.Host/FileStorageService.Host.csproj"

COPY . .

WORKDIR "/src/FileStorageService.Host"
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "FileStorageService.Host.dll"]
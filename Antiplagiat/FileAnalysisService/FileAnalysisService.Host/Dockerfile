FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5145

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FileAnalysisService.Entities/FileAnalysisService.Entities.csproj FileAnalysisService.Entities/
COPY FileAnalysisService.UseCases/FileAnalysisService.UseCases.csproj FileAnalysisService.UseCases/
COPY FileAnalysisService.Infrastructure/FileAnalysisService.Infrastructure.csproj FileAnalysisService.Infrastructure/
COPY FileAnalysisService.Presentation/FileAnalysisService.Presentation.csproj FileAnalysisService.Presentation/
COPY FileAnalysisService.Host/FileAnalysisService.Host.csproj FileAnalysisService.Host/

RUN dotnet restore "FileAnalysisService.Host/FileAnalysisService.Host.csproj"

COPY . .

RUN dotnet publish "FileAnalysisService.Host/FileAnalysisService.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5145
ENTRYPOINT ["dotnet", "FileAnalysisService.Host.dll"]
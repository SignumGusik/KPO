FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FileAnalysisService.Entities/FileAnalysisService.Entities.csproj", "FileAnalysisService.Entities/"]
COPY ["FileAnalysisService.UseCases/FileAnalysisService.UseCases.csproj", "FileAnalysisService.UseCases/"]
COPY ["FileAnalysisService.Infrastructure/FileAnalysisService.Infrastructure.csproj", "FileAnalysisService.Infrastructure/"]
COPY ["FileAnalysisService.Presentation/FileAnalysisService.Presentation.csproj", "FileAnalysisService.Presentation/"]
COPY ["FileAnalysisService.Host/FileAnalysisService.Host.csproj", "FileAnalysisService.Host/"]

RUN dotnet restore "FileAnalysisService.Host/FileAnalysisService.Host.csproj"

COPY . .

WORKDIR "/src/FileAnalysisService.Host"
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "FileAnalysisService.Host.dll"]
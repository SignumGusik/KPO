FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ApiGateway.Entities/ApiGateway.Entities.csproj", "ApiGateway.Entities/"]
COPY ["ApiGateway.UseCases/ApiGateway.UseCases.csproj", "ApiGateway.UseCases/"]
COPY ["ApiGateway.Infrastructure/ApiGateway.Infrastructure.csproj", "ApiGateway.Infrastructure/"]
COPY ["ApiGateway.Presentation/ApiGateway.Presentation.csproj", "ApiGateway.Presentation/"]
COPY ["ApiGateway.Host/ApiGateway.Host.csproj", "ApiGateway.Host/"]

RUN dotnet restore "ApiGateway.Host/ApiGateway.Host.csproj"

COPY . .

WORKDIR "/src/ApiGateway.Host"
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "ApiGateway.Host.dll"]
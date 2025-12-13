FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5090

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ApiGateway.Entities/ApiGateway.Entities.csproj ApiGateway.Entities/
COPY ApiGateway.UseCases/ApiGateway.UseCases.csproj ApiGateway.UseCases/
COPY ApiGateway.Infrastructure/ApiGateway.Infrastructure.csproj ApiGateway.Infrastructure/
COPY ApiGateway.Presentation/ApiGateway.Presentation.csproj ApiGateway.Presentation/
COPY ApiGateway.Host/ApiGateway.Host.csproj ApiGateway.Host/

RUN dotnet restore "ApiGateway.Host/ApiGateway.Host.csproj"

COPY . .

RUN dotnet publish "ApiGateway.Host/ApiGateway.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5090
ENTRYPOINT ["dotnet", "ApiGateway.Host.dll"]
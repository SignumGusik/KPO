# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY ["OrdersService.csproj", "./"]
RUN dotnet restore "./OrdersService.csproj"

# copy everything else and publish
COPY . .
RUN dotnet publish "OrdersService.csproj" -c Release -o /app/publish /p:UseWorkspace=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrdersService.dll"]
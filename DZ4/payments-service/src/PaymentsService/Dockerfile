# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PaymentsService.csproj", "./"]
RUN dotnet restore "./PaymentsService.csproj"

COPY . .
RUN dotnet publish "PaymentsService.csproj" -c Release -o /app/publish /p:UseWorkspace=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentsService.dll"]
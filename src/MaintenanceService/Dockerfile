FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/MaintenanceService/MaintenanceService.csproj", "src/MaintenanceService/"]
RUN dotnet restore "src/MaintenanceService/MaintenanceService.csproj"

COPY . .
WORKDIR "/src/src/MaintenanceService"
RUN dotnet build "MaintenanceService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MaintenanceService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MaintenanceService.dll"]
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/GarageService/GarageService.csproj", "src/GarageService/"]
RUN dotnet restore "src/GarageService/GarageService.csproj"

COPY . .
WORKDIR "/src/src/GarageService"
RUN dotnet build "GarageService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GarageService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GarageService.dll"]
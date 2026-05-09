FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ClubService/ClubService.csproj", "src/ClubService/"]
RUN dotnet restore "src/ClubService/ClubService.csproj"
COPY . .
WORKDIR "/src/src/ClubService"
RUN dotnet build "ClubService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClubService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClubService.dll"]
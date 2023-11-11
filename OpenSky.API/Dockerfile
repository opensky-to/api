FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
RUN apt-get update && apt-get install -y libgdiplus
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OpenSky.API/OpenSky.API.csproj", "OpenSky.API/"]
RUN dotnet restore "OpenSky.API/OpenSky.API.csproj"
COPY . .
WORKDIR "/src/OpenSky.API"
RUN dotnet build "OpenSky.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenSky.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenSky.API.dll"]
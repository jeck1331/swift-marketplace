FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Api/ApiService.WebApi/ApiService.WebApi.csproj", "src/Api/ApiService.WebApi/"]
RUN dotnet restore "src/Api/ApiService.WebApi/ApiService.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Api/ApiService.WebApi"
RUN dotnet build "./ApiService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApiService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiService.WebApi.dll"]

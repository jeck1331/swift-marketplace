FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Identity/IdentityService.WebApi/IdentityService.WebApi.csproj", "src/Identity/IdentityService.WebApi/"]
RUN dotnet restore "src/Identity/IdentityService.WebApi/IdentityService.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Identity/IdentityService.WebApi"
RUN dotnet build "./IdentityService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IdentityService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityService.WebApi.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Personal/PersonalService.WebApi/PersonalService.WebApi.csproj", "src/Personal/PersonalService.WebApi/"]
RUN dotnet restore "src/Personal/PersonalService.WebApi/PersonalService.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Personal/PersonalService.WebApi"
RUN dotnet build "./PersonalService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PersonalService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PersonalService.WebApi.dll"]

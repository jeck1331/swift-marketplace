FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./src/CatalogService/CatalogService.API/CatalogService.API.csproj
./src/CatalogService/CatalogService.API/
COPY ./src/CatalogService/CatalogService.Migrations/CatalogService.Migrations.csproj
./src/CatalogService/CatalogService.Migrations/
RUN dotnet restore ./src/CatalogService/CatalogService.API/CatalogService.API.csproj

COPY . .
RUN dotnet publish ./src/CatalogService/CatalogService.API/CatalogService.API csproj -c Release -o /out


FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CatalogService.API.dll"]
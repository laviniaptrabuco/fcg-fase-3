FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/FCG.Catalog.Domain/FCG.Catalog.Domain.csproj src/FCG.Catalog.Domain/
COPY src/FCG.Catalog.Application/FCG.Catalog.Application.csproj src/FCG.Catalog.Application/
COPY src/FCG.Catalog.Infrastructure/FCG.Catalog.Infrastructure.csproj src/FCG.Catalog.Infrastructure/
COPY src/FCG.CatalogAPI/FCG.CatalogAPI.csproj src/FCG.CatalogAPI/
RUN dotnet restore src/FCG.CatalogAPI/FCG.CatalogAPI.csproj

COPY . .
RUN dotnet publish src/FCG.CatalogAPI/FCG.CatalogAPI.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FCG.CatalogAPI.dll"]

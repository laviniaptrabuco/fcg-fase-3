FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/FCG.Users.Domain/FCG.Users.Domain.csproj src/FCG.Users.Domain/
COPY src/FCG.Users.Application/FCG.Users.Application.csproj src/FCG.Users.Application/
COPY src/FCG.Users.Infrastructure/FCG.Users.Infrastructure.csproj src/FCG.Users.Infrastructure/
COPY src/FCG.UsersAPI/FCG.UsersAPI.csproj src/FCG.UsersAPI/
RUN dotnet restore src/FCG.UsersAPI/FCG.UsersAPI.csproj

COPY . .
RUN dotnet publish src/FCG.UsersAPI/FCG.UsersAPI.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FCG.UsersAPI.dll"]

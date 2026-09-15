FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/FCG.PaymentsAPI/FCG.PaymentsAPI.csproj src/FCG.PaymentsAPI/
RUN dotnet restore src/FCG.PaymentsAPI/FCG.PaymentsAPI.csproj

COPY . .
RUN dotnet publish src/FCG.PaymentsAPI/FCG.PaymentsAPI.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.PaymentsAPI.dll"]

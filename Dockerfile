FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY src ./src

RUN dotnet restore src/RashePharma.Api/RashePharma.Api.csproj

RUN dotnet publish src/RashePharma.Api/RashePharma.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["sh", "-c", "dotnet RashePharma.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
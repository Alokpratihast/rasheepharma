FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY src ./src

COPY frontend/public/images/Rashelifescience.png ./frontend/public/images/Rashelifescience.png

RUN dotnet restore src/RashePharma.Api/RashePharma.Api.csproj

RUN dotnet publish src/RashePharma.Api/RashePharma.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=45s --retries=3 \
    CMD curl --fail http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["sh", "-c", "dotnet RashePharma.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY src ./src
COPY frontend ./frontend

RUN dotnet restore src/RashePharma.EmailWorker/RashePharma.EmailWorker.csproj

RUN dotnet publish src/RashePharma.EmailWorker/RashePharma.EmailWorker.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RashePharma.EmailWorker.dll"]

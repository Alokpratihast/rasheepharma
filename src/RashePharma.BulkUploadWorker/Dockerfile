FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/RashePharma.BulkUploadWorker/RashePharma.BulkUploadWorker.csproj
RUN dotnet publish src/RashePharma.BulkUploadWorker/RashePharma.BulkUploadWorker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RashePharma.BulkUploadWorker.dll"]

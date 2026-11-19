using RashePharma.Infrastructure.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddBulkUploadWorkerServices(builder.Configuration);
await builder.Build().RunAsync();

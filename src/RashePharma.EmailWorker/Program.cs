using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Infrastructure;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;
using RashePharma.Infrastructure.Services;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required by the email worker.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailJobRepository, EmailJobRepository>();
builder.Services.AddScoped<IEmailOutboxRepository, EmailOutboxRepository>();
builder.Services.AddScoped<IEmailDeliveryEventRepository, EmailDeliveryEventRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailJobService, EmailJobService>();

builder.Services.AddAzureServiceBus(builder.Configuration);
builder.Services.AddEmailJobConsumer(builder.Configuration);

await builder.Build().RunAsync();

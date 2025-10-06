using DnDAgency.EmailWorker;
using DnDAgency.EmailWorker.Infrastructure;
using DnDAgency.EmailWorker.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;

//  Web Application (Hangfire Dashboard)
var builder = WebApplication.CreateBuilder(args);

// Hangfire + PostgreSQL
builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(
            builder.Configuration.GetConnectionString("DefaultConnection")
        );
    });
});

// Hangfire Server
builder.Services.AddHangfireServer();

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddScoped<ReminderService>();

var app = builder.Build();


app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllAuthorizationFilter() }
});

app.Run("http://localhost:5001");
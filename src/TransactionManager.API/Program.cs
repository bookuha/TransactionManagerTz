using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TransactionManager.API.Middlewares;
using TransactionManager.Data;
using TransactionManager.Data.Database;
using TransactionManager.Logic;
using TransactionManager.Logic.DapperHelpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<TransactionManagerDbContext>(o =>
    o.UseNpgsql(builder.Configuration["PostgresConnectionString"]).EnableSensitiveDataLogging());
builder.Services.AddTransient<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddTransient<ITransactionsService, TransactionsService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Legiosoft Test Task for C# Junior Developer",
        Description = "An ASP.NET Core Web API for managing transactions",
        Contact = new OpenApiContact
        {
            Name = "Author's Telegram (click to open)",
            Url = new Uri("https://t.me/sharpenjoyer")
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

Dapper.SqlMapper.AddTypeHandler(new LocationTypeHandler());

var app = builder.Build();

using var scope = app.Services.CreateScope();
await using var dbContext = scope.ServiceProvider.GetRequiredService<TransactionManagerDbContext>();
await dbContext.Database.MigrateAsync();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program
{
}
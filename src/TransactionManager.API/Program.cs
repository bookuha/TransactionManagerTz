using Microsoft.EntityFrameworkCore;
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
builder.Services.AddSwaggerGen();

Dapper.SqlMapper.AddTypeHandler(new LocationTypeHandler());

var app = builder.Build();

using var scope = app.Services.CreateScope();
await using var dbContext = scope.ServiceProvider.GetRequiredService<TransactionManagerDbContext>();
await dbContext.Database.MigrateAsync();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
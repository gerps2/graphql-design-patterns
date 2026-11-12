using System.Reflection;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;
using Microsoft.EntityFrameworkCore;

[assembly: Module("Corretora")]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CorretoraDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("Corretora"))
    .UseSnakeCaseNamingConvention());
builder.Services.AddScoped<IBanco, BancoPostgres>();
builder.Services.AddMessaging(Assembly.GetExecutingAssembly());

builder.Services
    .AddGraphQLServer()
    .AddCorretora();

var app = builder.Build();

app.MapGraphQL();

app.Run();

public partial class Program;

using System.Reflection;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;
using Corretora.Api.Database.Interceptors;
using Microsoft.EntityFrameworkCore;

[assembly: Module("Corretora")]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CorretoraDbContext>(options =>
{
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("Corretora"))
        .UseSnakeCaseNamingConvention();

    if (builder.Configuration.GetValue<bool>("Aula:FalharAoGravarOrdem"))
        options.AddInterceptors(new FalhaEntreGravacoesInterceptor());
});
builder.Services.AddScoped<IBanco, BancoPostgres>();
builder.Services.AddMessaging(Assembly.GetExecutingAssembly());

builder.Services
    .AddGraphQLServer()
    .AddCorretora()
    // Cada campo da mutation ganha o seu escopo e o seu DbContext.
    .ModifyOptions(options => options.DefaultMutationDependencyInjectionScope = DependencyInjectionScope.Resolver);

var app = builder.Build();

app.MapGraphQL();

app.Run();

public partial class Program;

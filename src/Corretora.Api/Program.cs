using System.Reflection;
using Corretora.Api.Common.Decorators;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;
using Corretora.Api.Database.Interceptors;
using Corretora.Api.Features.Ordens.ObterDono;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

[assembly: Module("Corretora")]

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ConcorrenciaOptions>(builder.Configuration.GetSection("Concorrencia"));
builder.Services.AddContagemDeConsultas();
builder.Services.AddDbContext<CorretoraDbContext>((services, options) =>
{
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("Corretora"))
        .UseSnakeCaseNamingConvention()
        .ReplaceService<IModelCacheKeyFactory, ModeloPorEstrategia>()
        .AddInterceptors(ContadorDeConsultasInterceptor.DoRequestAtual(services));

    // Cada SQL enviado aparece no terminal. Ligado só em desenvolvimento (appsettings.Development.json).
    if (builder.Configuration.GetValue<bool>("Aula:MostrarSql"))
        options.LogTo(Console.WriteLine, [DbLoggerCategory.Database.Name], LogLevel.Information);

    if (builder.Configuration.GetValue<bool>("Aula:FalharAoGravarOrdem"))
        options.AddInterceptors(new FalhaEntreGravacoesInterceptor());
});
// As leituras (IBanco) ficam no EF Core com AsNoTracking. A escrita usa o provedor escolhido.
builder.Services.AddScoped<IBanco, BancoPostgres>();
builder.Services.AddPersistencia(builder.Configuration);
builder.Services.AddMessaging(Assembly.GetExecutingAssembly());
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Como no material: o DataLoader escrito à mão, registrado como qualquer serviço.
builder.Services.AddScoped<UsuarioBatchDataLoader>();

// O pipeline do IMediator. Cada Decorate embrulha o que já existe, então o último fica por fora:
// Logging → Validation → Handler.
builder.Services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandler<,>));
builder.Services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));
builder.Services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));

builder.Services
    .AddGraphQLServer()
    .AddCorretora()
    .AddMutationConventions(applyToAllMutations: true)
    // Cada campo da mutation ganha o seu escopo e o seu DbContext.
    .ModifyOptions(options => options.DefaultMutationDependencyInjectionScope = DependencyInjectionScope.Resolver);

var app = builder.Build();

app.UseContagemDeConsultas();
app.MapGraphQL();

app.Run();

public partial class Program;

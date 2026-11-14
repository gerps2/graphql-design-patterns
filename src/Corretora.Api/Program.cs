using System.Reflection;
using Corretora.Api.Common.Decorators;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Database.Interceptors;
using Corretora.Api.Features.Cotacoes;
using Corretora.Api.Database;
using Corretora.Api.Common.Events;
using Corretora.Api.GraphQL.Errors;
using Corretora.Api.GraphQL.Subscriptions;
using Corretora.Api.Observabilidade;
using FluentValidation;
using HotChocolate;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

[assembly: Module("Corretora")]

var builder = WebApplication.CreateBuilder(args);

builder.AddTelemetria();

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

    if (builder.Configuration.GetValue<bool>("Aula:FalharRentabilidade"))
        options.AddInterceptors(new FalhaNaRentabilidadeInterceptor());
});
// As leituras (IBanco) ficam no EF Core com AsNoTracking. A escrita usa o provedor escolhido.
builder.Services.AddScoped<IBanco, BancoPostgres>();
builder.Services.AddPersistencia(builder.Configuration);
// Cache em dois níveis: L1 na memória da API e L2 no Redis, quando houver conexão (ConnectionStrings:Redis).
// Sem o Redis, o HybridCache continua funcionando só com a memória.
var redis = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redis))
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redis;
        options.InstanceName = "corretora:";
    });
builder.Services.AddHybridCache();
// A bolsa simulada da Aula 7: os preços em memória e o relógio que muda um ativo por vez.
builder.Services.AddSingleton<MercadoSimulado>();
builder.Services.AddSingleton<IEventPublisher, HotChocolateEventPublisher>();
builder.Services.AddHostedService<SimuladorDeMercado>();
builder.Services.AddMessaging(Assembly.GetExecutingAssembly());
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// O pipeline do IMediator. Cada Decorate embrulha o que já existe, então o último fica por fora.
// Command: Logging → Validation → CacheInvalidation (só o command marcado) → Handler.
// Query:   Logging → Caching (só age na query com chave) → Handler.
builder.Services.Decorate(typeof(ICommandHandler<,>), typeof(CacheInvalidationDecorator.CommandHandler<,>));
builder.Services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandler<,>));
builder.Services.Decorate(typeof(IQueryHandler<,>), typeof(CachingDecorator.QueryHandler<,>));
builder.Services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));
builder.Services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));

builder.Services
    .AddGraphQLServer()
    .AddCorretora()
    .AddMutationConventions(applyToAllMutations: true)
    // Subscriptions: os eventos passam por um provedor. Em memória, só esta instância da API enxerga.
    .AddInMemorySubscriptions()
    // A interface Error do schema ganha o code, além da message.
    .AddErrorInterfaceType<ICorretoraError>()
    // Proteção: no GraphQL o cliente monta a query, e estes limites dizem até onde ele pode ir.
    // Profundidade: até 5 níveis de campo. A tela mais funda da corretora usa 3.
    .AddMaxExecutionDepthRule(5)
    // Custo: o Hot Chocolate 16 já calcula antes de executar e barra acima de 1.000. Fica escrito, e não por acaso.
    .ModifyCostOptions(options => options.MaxFieldCost = 1_000)
    // Introspecção (o schema inteiro para quem pedir): ligada em desenvolvimento, desligada no resto.
    // Na 16.7 o método é DisableIntrospection; a documentação atual (16.8) já fala em AllowIntrospection.
    .DisableIntrospection(!builder.Configuration.GetValue("GraphQL:PermitirIntrospeccao", builder.Environment.IsDevelopment()))
    // Cada campo da mutation ganha o seu escopo e o seu DbContext.
    .ModifyOptions(options => options.DefaultMutationDependencyInjectionScope = DependencyInjectionScope.Resolver)
    // Detalhe da exceção (mensagem interna e stack) na resposta: só quando alguém liga de propósito.
    .ModifyRequestOptions(options => options.IncludeExceptionDetails = builder.Configuration.GetValue<bool>("GraphQL:DetalhesDaExcecao"))
    // O filtro de erros vive nos serviços do schema. O log vem da aplicação: o AddApplicationService empresta.
    .AddApplicationService<ILoggerFactory>()
    .AddErrorFilter<CorretoraErrorFilter>()
    // Cada operação GraphQL vira um trace com o nome da operação, e cada resolver, um trecho dele.
    .AddInstrumentation();

var app = builder.Build();

app.UseContagemDeConsultas();
// A subscription chega por WebSocket: a conexão fica aberta e o servidor manda quando tiver novidade.
app.UseWebSockets();
app.MapGraphQL();

app.Run();

public partial class Program;

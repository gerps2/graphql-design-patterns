using Corretora.Bff.Servicos;
using HotChocolate;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

[assembly: Module("Bff")]

// O BFF da corretora: uma porta GraphQL para o app, na frente da API e do serviço de notificações.
var builder = WebApplication.CreateBuilder(args);

// Os dois serviços que o BFF chama, cada um com o seu HttpClient. Os endereços vêm do appsettings.
builder.Services.AddHttpClient<CorretoraApiClient>(http =>
    http.BaseAddress = new Uri(builder.Configuration["Servicos:CorretoraApi"]!));
builder.Services.AddHttpClient<NotificacoesClient>(http =>
    http.BaseAddress = new Uri(builder.Configuration["Servicos:Notificacoes"]!))
    .AddStandardResilienceHandler(opcoes =>
    {
        opcoes.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
        opcoes.Retry.MaxRetryAttempts = 2;
        opcoes.Retry.Delay = TimeSpan.FromMilliseconds(200);
        opcoes.CircuitBreaker.MinimumThroughput = 5;
        opcoes.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        opcoes.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
    });

builder.Services
    .AddGraphQLServer()
    .AddBff()
    .AddInstrumentation();

// Trace em OTLP: a chamada do app, a operação GraphQL e cada ida aos serviços, num trace só (Aula 6).
if (!string.IsNullOrEmpty(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("corretora-bff"))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddHotChocolateInstrumentation()
            .AddHttpClientInstrumentation()    // cada chamada aos serviços, com o traceparent no cabeçalho
            .AddOtlpExporter());

var app = builder.Build();

app.MapGraphQL();

app.Run();

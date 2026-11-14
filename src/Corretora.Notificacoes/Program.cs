using Corretora.Notificacoes;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;

// O serviço de notificações, do outro time: REST, pequeno, com os avisos de cada investidor.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));
builder.Services.AddSingleton<CaixaDeAvisos>();
builder.Services.AddHostedService<OuvinteDeOrdens>();

// Trace em OTLP, como a API: quem chama este serviço aparece no mesmo trace (Aula 6).
if (!string.IsNullOrEmpty(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("corretora-notificacoes"))
        .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddOtlpExporter());

var app = builder.Build();

// GET /notificacoes?usuarioIds=1&usuarioIds=2: os avisos de vários usuários numa chamada só.
// Cada pedido aparece no terminal, para contar quantas vezes o serviço foi chamado.
app.MapGet("/notificacoes", (int[] usuarioIds, CaixaDeAvisos caixa, ILogger<CaixaDeAvisos> logger) =>
{
    logger.LogInformation("Pedido de avisos: usuários {Usuarios}", string.Join(", ", usuarioIds));
    return caixa.DosUsuarios(usuarioIds);
});

app.Run();

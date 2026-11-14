using Corretora.Api.Common.Telemetria;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Corretora.Api.Observabilidade;

// Liga os três sinais do OpenTelemetry (trace, métrica e log) e manda tudo em OTLP para um endereço só.
// Quem decide para onde vai cada sinal é o Collector (deployments/observabilidade), não a API.
// Sem OTEL_EXPORTER_OTLP_ENDPOINT na configuração, nada é ligado: a API roda igual, sem telemetria.
public static class TelemetriaExtensions
{
    public static WebApplicationBuilder AddTelemetria(this WebApplicationBuilder builder)
    {
        if (string.IsNullOrEmpty(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
            return builder;

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("corretora-api"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()     // o request HTTP que chegou
                .AddHotChocolateInstrumentation()   // a operação GraphQL, a validação e cada resolver
                .AddNpgsql()                        // cada SQL que foi ao Postgres
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()     // requests por segundo e duração
                .AddRuntimeInstrumentation()        // memória e GC do .NET
                .AddMeter(CorretoraMetricas.Nome)   // as nossas: erros por código
                .AddOtlpExporter());

        // O mesmo log do terminal, com o traceId junto, vai para o Loki.
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.AddOtlpExporter();
        });

        return builder;
    }
}

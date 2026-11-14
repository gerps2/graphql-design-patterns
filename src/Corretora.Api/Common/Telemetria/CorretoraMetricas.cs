using System.Diagnostics.Metrics;

namespace Corretora.Api.Common.Telemetria;

// As métricas que são nossas, e não do framework. O nome do Meter é o que o OpenTelemetry assina.
public static class CorretoraMetricas
{
    public const string Nome = "Corretora";

    private static readonly Meter Meter = new(Nome);

    // Quantos erros a API devolveu, por código: SALDO_INSUFICIENTE, VALIDATION_ERROR, INTERNAL_ERROR...
    // No Prometheus vira corretora_erros_total{codigo="..."}.
    private static readonly Counter<long> Erros = Meter.CreateCounter<long>("corretora.erros", description: "Erros devolvidos ao cliente, por código");

    public static void ContarErro(string codigo) =>
        Erros.Add(1, new KeyValuePair<string, object?>("codigo", codigo));
}

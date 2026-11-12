namespace Corretora.Api.Database.Interceptors;

// As consultas de um request. Os resolvers rodam em paralelo, então a soma é atômica.
public sealed class ContagemDeConsultas
{
    public const string Header = "X-Consultas-Banco";

    private int _total;

    public int Total => _total;

    public void Somar() => Interlocked.Increment(ref _total);

    public static ContagemDeConsultas? Do(HttpContext? http) =>
        http?.Items[typeof(ContagemDeConsultas)] as ContagemDeConsultas;
}

public static class ContagemDeConsultasExtensions
{
    public static IServiceCollection AddContagemDeConsultas(this IServiceCollection services) =>
        services.AddHttpContextAccessor();

    // Abre a contagem no começo do request. No fim, o total vai para o header (o k6 e os testes leem) e para o log.
    public static IApplicationBuilder UseContagemDeConsultas(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var contagem = new ContagemDeConsultas();
            context.Items[typeof(ContagemDeConsultas)] = contagem;
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[ContagemDeConsultas.Header] = contagem.Total.ToString();
                return Task.CompletedTask;
            });

            await next(context);

            if (contagem.Total > 0)
                context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Banco")
                    .LogInformation("Banco: {Consultas} consultas neste request", contagem.Total);
        });
}

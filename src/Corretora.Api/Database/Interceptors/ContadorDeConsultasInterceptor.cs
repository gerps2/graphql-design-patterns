using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Corretora.Api.Database.Interceptors;

// Toda consulta que sai para o banco passa por aqui, e o interceptor soma um na contagem do request.
// Cada DbContext ganha o seu, criado dentro do request: a consulta conta mesmo se rodar em outra thread.
public sealed class ContadorDeConsultasInterceptor(ContagemDeConsultas? contagem) : DbCommandInterceptor
{
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken ct = default)
    {
        contagem?.Somar();
        return base.ReaderExecutingAsync(command, eventData, result, ct);
    }

    // Fora de um request (migrations, testes), não há contagem e o interceptor não faz nada.
    public static ContadorDeConsultasInterceptor DoRequestAtual(IServiceProvider services) =>
        new(ContagemDeConsultas.Do(services.GetRequiredService<IHttpContextAccessor>().HttpContext));
}

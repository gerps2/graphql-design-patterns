using GreenDonut;
using HotChocolate;
using HotChocolate.Types;
using Polly;

namespace Corretora.Bff.Telas.Inicio;

[ObjectType<Inicio>]
public static partial class InicioCampos
{
    // O patrimônio é calculado aqui, no BFF. O app só mostra.
    public static decimal GetPatrimonio([Parent] Inicio inicio) =>
        inicio.SaldoDisponivel + inicio.Carteira.Sum(p => p.Valor);

    // Os avisos moram em outro serviço. Para o app, é só mais um campo da tela.
    // O campo pode vir nulo: se o serviço cair, a tela sai sem os avisos, e não sem tudo.
    public static async Task<IReadOnlyList<Aviso>?> GetAvisosAsync(
        [Parent] Inicio inicio, IAvisosByUsuarioIdDataLoader avisosByUsuarioId, CancellationToken ct, int ultimos = 10)
    {
        try
        {
            var avisos = await avisosByUsuarioId.LoadAsync(inicio.UsuarioId, ct) ?? [];
            return avisos.Take(ultimos).Select(a => new Aviso(a.Texto, a.CriadoEm)).ToList();
        }
        catch (Exception e) when (e is HttpRequestException or ExecutionRejectedException)
        {
            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage("Os avisos estão indisponíveis agora. O resto da tela continua valendo.")
                .SetCode("NOTIFICACOES_INDISPONIVEL")
                .Build());
        }
    }
}

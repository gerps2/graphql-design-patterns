using Corretora.Bff.Servicos;
using HotChocolate.Types;

namespace Corretora.Bff.Telas.Inicio;

[QueryType]
public static partial class InicioQuery
{
    // A tela inicial de um investidor. Por enquanto, só com o que vem da API da corretora.
    public static async Task<Inicio?> GetInicioAsync(int usuarioId, CorretoraApiClient api, CancellationToken ct)
    {
        var usuario = await api.ObterUsuarioAsync(usuarioId, ct);
        return usuario is null ? null : Inicio.De(usuario);
    }

    // A tela do assessor: todos os investidores, cada um com o seu resumo.
    public static async Task<IReadOnlyList<Inicio>> GetInvestidoresAsync(CorretoraApiClient api, CancellationToken ct) =>
        (await api.ObterUsuariosAsync(ct)).Select(Inicio.De).ToList();
}

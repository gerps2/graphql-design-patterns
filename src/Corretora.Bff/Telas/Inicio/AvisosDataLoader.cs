using Corretora.Bff.Servicos;
using GreenDonut;

namespace Corretora.Bff.Telas.Inicio;

// DataLoader de grupo entre serviços: junta os usuários pedidos no mesmo request
// e chama o serviço de notificações uma vez só, no endpoint em lote.
public static class AvisosDataLoader
{
    [DataLoader]
    public static async Task<Dictionary<int, AvisoDoServico[]>> GetAvisosByUsuarioIdAsync(
        IReadOnlyList<int> usuarioIds,
        NotificacoesClient notificacoes,
        CancellationToken ct) =>
        (await notificacoes.DosUsuariosAsync(usuarioIds, ct))
            .GroupBy(a => a.UsuarioId)
            .ToDictionary(g => g.Key, g => g.ToArray());
}

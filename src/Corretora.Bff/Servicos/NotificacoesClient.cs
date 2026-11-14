using System.Net.Http.Json;

namespace Corretora.Bff.Servicos;

// Fala com o serviço de notificações por REST, no formato dele.
public sealed class NotificacoesClient(HttpClient http)
{
    // Os avisos de vários usuários numa chamada só: /notificacoes?usuarioIds=1&usuarioIds=2
    public async Task<IReadOnlyList<AvisoDoServico>> DosUsuariosAsync(IEnumerable<int> usuarioIds, CancellationToken ct)
    {
        var consulta = string.Join("&", usuarioIds.Select(id => $"usuarioIds={id}"));
        return await http.GetFromJsonAsync<List<AvisoDoServico>>($"/notificacoes?{consulta}", ct) ?? [];
    }
}

// O formato do serviço de notificações. Também não sai do BFF.
public sealed record AvisoDoServico(int UsuarioId, string Texto, DateTimeOffset CriadoEm);

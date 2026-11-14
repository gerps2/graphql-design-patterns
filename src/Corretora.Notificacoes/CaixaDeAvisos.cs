using System.Collections.Concurrent;

namespace Corretora.Notificacoes;

// Os avisos de cada usuário, em memória. Num serviço de verdade seria um banco próprio, do time de notificações.
public sealed class CaixaDeAvisos
{
    private readonly ConcurrentDictionary<int, List<Aviso>> _avisos = new(
        Enumerable.Range(1, 5).Select(id => KeyValuePair.Create(id,
            new List<Aviso> { new(id, "Bem-vindo à corretora. A sua conta está aberta.", DateTimeOffset.UtcNow.AddDays(-1)) })));

    public void Guardar(Aviso aviso)
    {
        var lista = _avisos.GetOrAdd(aviso.UsuarioId, _ => []);
        lock (lista)
            lista.Add(aviso);
    }

    // Os avisos de vários usuários de uma vez, do mais novo para o mais antigo.
    public IReadOnlyList<Aviso> DosUsuarios(IEnumerable<int> usuarioIds) =>
        usuarioIds.Distinct()
            .SelectMany(id => _avisos.TryGetValue(id, out var lista) ? Copiar(lista) : [])
            .OrderByDescending(a => a.CriadoEm)
            .ToList();

    private static List<Aviso> Copiar(List<Aviso> lista)
    {
        lock (lista)
            return [.. lista];
    }
}

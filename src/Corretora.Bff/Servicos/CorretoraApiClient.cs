using System.Net.Http.Json;

namespace Corretora.Bff.Servicos;

// Fala com a API da corretora por GraphQL sobre HTTP: um POST com a query e as variáveis no corpo.
// O BFF não referencia o projeto da API. Conhece só o contrato: a query e o formato da resposta.
public sealed class CorretoraApiClient(HttpClient http)
{
    private const string CamposDoUsuario = "id nome saldoDisponivel carteira { ativo quantidade saldo }";

    public async Task<UsuarioDaApi?> ObterUsuarioAsync(int id, CancellationToken ct) =>
        (await PerguntarAsync<RespostaUsuario>(
            $"query Usuario($id: Int!) {{ usuario(id: $id) {{ {CamposDoUsuario} }} }}", new { id }, ct)).Usuario;

    public async Task<IReadOnlyList<UsuarioDaApi>> ObterUsuariosAsync(CancellationToken ct) =>
        (await PerguntarAsync<RespostaUsuarios>($"query Usuarios {{ usuarios {{ {CamposDoUsuario} }} }}", null, ct)).Usuarios;

    private async Task<T> PerguntarAsync<T>(string query, object? variables, CancellationToken ct)
    {
        using var pedido = new HttpRequestMessage(HttpMethod.Post, "/graphql") { Content = JsonContent.Create(new { query, variables }) };
        pedido.Headers.Accept.ParseAdd("application/json");

        using var resposta = await http.SendAsync(pedido, ct);
        resposta.EnsureSuccessStatusCode();
        var corpo = await resposta.Content.ReadFromJsonAsync<RespostaGraphQL<T>>(ct);

        if (corpo?.Errors is [var erro, ..])
            throw new InvalidOperationException($"A API da corretora respondeu com erro: {erro.Message}");
        return corpo!.Data!;
    }

    private sealed record RespostaGraphQL<T>(T? Data, List<ErroGraphQL>? Errors);
    private sealed record ErroGraphQL(string Message);
    private sealed record RespostaUsuario(UsuarioDaApi? Usuario);
    private sealed record RespostaUsuarios(List<UsuarioDaApi> Usuarios);
}

// O formato da API da corretora, do jeito que ela responde. Não sai do BFF.
public sealed record UsuarioDaApi(int Id, string Nome, decimal SaldoDisponivel, List<PosicaoDaApi> Carteira);

public sealed record PosicaoDaApi(string Ativo, decimal Quantidade, decimal Saldo);

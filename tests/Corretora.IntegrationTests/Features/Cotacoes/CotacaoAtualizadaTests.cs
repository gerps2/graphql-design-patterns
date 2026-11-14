using Corretora.Api.Common.Events;
using Corretora.Api.Features.Cotacoes;
using Corretora.Api.Features.Cotacoes.CotacaoAtualizada;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Corretora.IntegrationTests.Features.Cotacoes;

// A subscription roda direto no executor do Hot Chocolate: assina, publica e lê o que chegou.
[Collection(nameof(ApiCollection))]
public class CotacaoAtualizadaTests(CorretoraApiFactory api)
{
    [Fact]
    public async Task CotacaoAtualizada_PrecoPublicadoNoTopicoDoAtivo_ChegaParaQuemAssinou()
    {
        // Arrange
        var executor = await api.Services.GetRequestExecutorAsync();
        var resultado = await executor.ExecuteAsync("subscription { cotacaoAtualizada(ativo: \"VALE3\") { ativo preco } }");
        await using var stream = resultado.ExpectResponseStream();
        await using var leitura = stream.ReadResultsAsync().GetAsyncEnumerator();
        var proximo = leitura.MoveNextAsync();
        var eventos = api.Services.GetRequiredService<IEventPublisher>();

        // Act
        await eventos.PublicarAsync(CotacaoAtualizadaTopico.De("VALE3"),
            new CotacaoOutput { Ativo = "VALE3", Preco = 99.99m, Variacao = 0m, Horario = DateTimeOffset.UtcNow });

        // Assert
        (await proximo.AsTask().WaitAsync(TimeSpan.FromSeconds(10))).Should().BeTrue();
        leitura.Current.ToJson().Should().Contain("\"ativo\": \"VALE3\"");
    }
}

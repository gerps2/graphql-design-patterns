using Corretora.Api.Common.Events;
using Corretora.Api.Features.Ordens;
using Corretora.Api.Features.Ordens.OrdemEnviada;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Corretora.IntegrationTests.Features.Ordens.OrdemEnviada;

// Duas instâncias da API no mesmo Redis: quem assina numa recebe o evento publicado na outra.
[Collection(nameof(ApiCollection))]
public class DuasInstanciasTests(CorretoraApiFactory api)
{
    [Fact]
    public async Task OrdemEnviada_PublicadaNaOutraInstancia_ChegaPeloRedis()
    {
        // Arrange
        var outraInstancia = api.Com(("Aula:Instancia", "2"));
        var executor = await api.Services.GetRequestExecutorAsync();
        var resultado = await executor.ExecuteAsync("subscription { ordemEnviada(usuarioId: 5) { ativo } }");
        await using var stream = resultado.ExpectResponseStream();
        await using var leitura = stream.ReadResultsAsync().GetAsyncEnumerator();
        var proximo = leitura.MoveNextAsync();
        await Task.Delay(TimeSpan.FromSeconds(1)); // a inscrição no canal do Redis é assíncrona
        var ordem = new OrdemOutput(1, 5, "HGLG11", 1m, 160m, 160m, DateTime.UtcNow);

        // Act
        await outraInstancia.Services.GetRequiredService<IEventPublisher>().PublicarAsync(OrdemEnviadaTopico.De(5), ordem);

        // Assert
        (await proximo.AsTask().WaitAsync(TimeSpan.FromSeconds(10))).Should().BeTrue();
        leitura.Current.ToJson().Should().Contain("\"ativo\": \"HGLG11\"");
    }
}

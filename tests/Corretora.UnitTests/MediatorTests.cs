using AwesomeAssertions;
using Corretora.Api.Common;
using Corretora.Api.Database;
using Corretora.Api.Features.Carteira;
using Microsoft.Extensions.DependencyInjection;

namespace Corretora.UnitTests;

public class MediatorTests
{
    [Fact]
    public async Task Entrega_a_query_ao_handler_certo()
    {
        var servicos = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IBanco>(new BancoDeTeste { Posicoes = [new Posicao(1, "PETR4", 10, 750m)] })
            .AddQueries(typeof(ObterCarteira).Assembly)
            .BuildServiceProvider();

        var mediator = servicos.GetRequiredService<IMediator>();
        var carteira = await mediator.Send(new ObterCarteira.Query(1));

        carteira.Should().ContainSingle(p => p.Ativo == "PETR4");
    }
}

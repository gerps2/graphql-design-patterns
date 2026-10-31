using AwesomeAssertions;
using Corretora.Api.Features.Transacoes;

namespace Corretora.UnitTests;

public class ObterTransacoesTests
{
    [Fact]
    public async Task Mais_recentes_primeiro()
    {
        var banco = new BancoDeTeste
        {
            Transacoes =
            [
                new Transacao(1, 1, "PETR4", TipoTransacao.Compra, 100m, new DateOnly(2026, 10, 20)),
                new Transacao(1, 2, "ITUB4", TipoTransacao.Venda, 100m, new DateOnly(2026, 10, 28)),
            ],
        };
        var handler = new ObterTransacoes.Handler(banco);

        var transacoes = await handler.Handle(new ObterTransacoes.Query(1), CancellationToken.None);

        transacoes.Select(t => t.Id).Should().Equal(2, 1);
    }
}

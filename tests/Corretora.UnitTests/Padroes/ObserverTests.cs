using Corretora.Api.Features.Cotacoes;

namespace Corretora.UnitTests.Padroes;

// O padrão Observer em C# puro, antes de qualquer biblioteca.
// A bolsa (o Subject) guarda a lista de quem quer ser avisado e avisa cada um quando o preço muda.
public class ObserverTests
{
    private sealed class Bolsa : IObservable<Cotacao>
    {
        private readonly List<IObserver<Cotacao>> _observadores = [];

        public IDisposable Subscribe(IObserver<Cotacao> observador)
        {
            _observadores.Add(observador);
            return new Cancelamento(() => _observadores.Remove(observador));
        }

        public void Publicar(Cotacao cotacao)
        {
            foreach (var observador in _observadores.ToList())
                observador.OnNext(cotacao);
        }
    }

    private sealed class Painel : IObserver<Cotacao>
    {
        public List<Cotacao> Recebidas { get; } = [];

        public void OnNext(Cotacao cotacao) => Recebidas.Add(cotacao);

        public void OnError(Exception error) { }

        public void OnCompleted() { }
    }

    private sealed class Cancelamento(Action cancelar) : IDisposable
    {
        public void Dispose() => cancelar();
    }

    private static readonly Cotacao Petr4 = new("PETR4", 38.69m, 0.49m, DateTimeOffset.UtcNow);

    [Fact]
    public void Publicar_DoisPaineisInscritos_OsDoisRecebem()
    {
        // Arrange
        var bolsa = new Bolsa();
        var web = new Painel();
        var celular = new Painel();
        bolsa.Subscribe(web);
        bolsa.Subscribe(celular);

        // Act
        bolsa.Publicar(Petr4);

        // Assert
        web.Recebidas.Should().ContainSingle().Which.Should().Be(Petr4);
        celular.Recebidas.Should().ContainSingle().Which.Should().Be(Petr4);
    }

    [Fact]
    public void Publicar_DepoisDeCancelar_ParaDeReceber()
    {
        // Arrange
        var bolsa = new Bolsa();
        var celular = new Painel();
        var inscricao = bolsa.Subscribe(celular);

        // Act
        inscricao.Dispose();
        bolsa.Publicar(Petr4);

        // Assert
        celular.Recebidas.Should().BeEmpty();
    }
}

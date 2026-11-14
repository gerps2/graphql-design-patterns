using System.Net;
using System.Text;

namespace Corretora.UnitTests.Bff;

// Um serviço HTTP de mentira: responde sempre o mesmo JSON e guarda os endereços que foram chamados.
public sealed class ServicoFalso(string json, HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
{
    public List<string> Chamadas { get; } = [];

    public HttpClient Cliente() => new(this) { BaseAddress = new Uri("http://servico") };

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Chamadas.Add(request.RequestUri!.PathAndQuery);
        return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
    }
}

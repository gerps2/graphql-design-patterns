namespace Corretora.Api.Features.Cotacoes.CotacaoAtualizada;

// Um tópico por ativo: quem assina a PETR4 não recebe a VALE3. É o grupo de WhatsApp por assunto.
public static class CotacaoAtualizadaTopico
{
    public const string Prefixo = "CotacaoAtualizada_";

    public static string De(string ativo) => Prefixo + ativo;
}

namespace Corretora.Api.Features.Ordens.OrdemEnviada;

// Um tópico por usuário: cada investidor ouve só as próprias ordens.
public static class OrdemEnviadaTopico
{
    public const string Prefixo = "OrdemEnviada_";

    public static string De(int usuarioId) => Prefixo + usuarioId;
}

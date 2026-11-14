using HotChocolate;

namespace Corretora.Api.Features.Ordens.OrdemEnviada;

// O app do investidor fica sabendo da ordem gravada na hora, mesmo que ela tenha saído de outro aparelho.
[SubscriptionType]
public static partial class OrdemEnviadaResolver
{
    [Subscribe]
    [Topic($"{OrdemEnviadaTopico.Prefixo}{{{nameof(usuarioId)}}}")]
    public static OrdemOutput OrdemEnviada(int usuarioId, [EventMessage] OrdemOutput ordem) => ordem;
}

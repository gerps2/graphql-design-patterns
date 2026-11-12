using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Ordens.ObterUltimasOrdens;

public sealed record ObterUltimasOrdensQuery(int Quantidade) : IQuery<List<OrdemOutput>>;

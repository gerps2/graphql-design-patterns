using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Ordens.ObterOrdens;

public sealed record ObterOrdensQuery(int UsuarioId) : IQuery<List<OrdemOutput>>;

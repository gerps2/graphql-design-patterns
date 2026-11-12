using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Contas.ObterSaldo;

public sealed record ObterSaldoQuery(int UsuarioId) : IQuery<decimal>;

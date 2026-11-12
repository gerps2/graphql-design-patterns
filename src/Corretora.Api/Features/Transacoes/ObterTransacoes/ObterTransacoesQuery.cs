using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Transacoes.ObterTransacoes;

public sealed record ObterTransacoesQuery(int UsuarioId) : IQuery<List<TransacaoOutput>>;

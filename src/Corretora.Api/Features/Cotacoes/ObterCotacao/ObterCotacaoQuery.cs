using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Cotacoes.ObterCotacao;

public sealed record ObterCotacaoQuery(string Ativo) : IQuery<CotacaoOutput?>;

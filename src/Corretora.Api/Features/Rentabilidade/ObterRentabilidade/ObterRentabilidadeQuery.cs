using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Rentabilidade.ObterRentabilidade;

public sealed record ObterRentabilidadeQuery(int UsuarioId) : IQuery<List<PosicaoRentabilidadeOutput>>;

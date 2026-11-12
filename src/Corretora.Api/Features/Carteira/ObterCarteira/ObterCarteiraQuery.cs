using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Carteira.ObterCarteira;

public sealed record ObterCarteiraQuery(int UsuarioId) : IQuery<List<PosicaoNaCarteiraOutput>>;

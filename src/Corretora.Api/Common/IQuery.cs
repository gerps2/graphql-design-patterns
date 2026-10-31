namespace Corretora.Api.Common;

// Uma pergunta ao sistema: quem a envia recebe TResposta e nada muda.
public interface IQuery<TResposta>;

public interface IQueryHandler<in TQuery, TResposta>
    where TQuery : IQuery<TResposta>
{
    Task<TResposta> Handle(TQuery query, CancellationToken ct);
}

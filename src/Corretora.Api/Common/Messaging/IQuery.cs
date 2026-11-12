namespace Corretora.Api.Common.Messaging;

// Uma pergunta ao sistema: quem a envia recebe TResponse e nada muda.
public interface IQuery<TResponse>;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken ct);
}

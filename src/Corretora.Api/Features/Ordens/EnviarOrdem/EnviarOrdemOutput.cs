namespace Corretora.Api.Features.Ordens.EnviarOrdem;

// O que o caso de uso devolve. No schema é o EnviarOrdemPayload, e o Hot Chocolate não embrulha de novo.
// A ordem é anulável como no payload que a convenção de mutation gera: na Aula 6, com erro, ela vem nula.
[GraphQLName("EnviarOrdemPayload")]
public sealed record EnviarOrdemOutput(OrdemOutput? Ordem);

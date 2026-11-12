namespace Corretora.Api.Database;

// Como duas ordens da mesma conta convivem. Escolhida em Concorrencia:Estrategia (padrão: Pessimista).
public enum EstrategiaDeConcorrencia
{
    // Lê e grava sem cuidado nenhum: a última gravação vence (lost update). Só para a medição.
    SemTrava,

    // SELECT ... FOR UPDATE: a segunda ordem espera a primeira terminar.
    Pessimista,

    // Ninguém espera; quem grava com a versão velha recebe conflito.
    Otimista,
}

public sealed class ConcorrenciaOptions
{
    public EstrategiaDeConcorrencia Estrategia { get; set; } = EstrategiaDeConcorrencia.Pessimista;
}

namespace Corretora.Api.Features.Contas;

// Regra de negócio quebrada: a ordem custa mais do que a conta tem.
public sealed class SaldoInsuficienteException(decimal disponivel, decimal necessario)
    : Exception($"Saldo insuficiente: disponível {disponivel}, necessário {necessario}.")
{
    public decimal Disponivel { get; } = disponivel;

    public decimal Necessario { get; } = necessario;
}

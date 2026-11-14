using Corretora.Api.Common.Results;

namespace Corretora.Api.Features.Contas;

// As falhas esperadas da conta. O código é estável: o app decide o que mostrar a partir dele.
public static class ContaErrors
{
    public static Error NaoEncontrada(int usuarioId) =>
        Error.NotFound("CONTA_NAO_ENCONTRADA", $"O usuário {usuarioId} não tem conta.");

    public static Error SaldoInsuficiente(decimal disponivel, decimal necessario) =>
        Error.Failure("SALDO_INSUFICIENTE", $"Saldo insuficiente: disponível {disponivel:N2}, necessário {necessario:N2}.");

    public static Error AlteradaPorOutraOrdem(int usuarioId) =>
        Error.Conflict("CONTA_ALTERADA", $"A conta do usuário {usuarioId} mudou enquanto a ordem era gravada. Tente de novo.");
}

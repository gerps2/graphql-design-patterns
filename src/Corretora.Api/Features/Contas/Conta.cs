using Corretora.Api.Common.Results;

namespace Corretora.Api.Features.Contas;

// O dinheiro disponível do usuário para comprar ativos.
public sealed class Conta
{
    private Conta()
    {
    }

    public int UsuarioId { get; private set; }

    public decimal Saldo { get; private set; }

    // A versão da linha. No Postgres é a coluna de sistema xmin, que muda a cada gravação.
    public uint Version { get; private set; }

    public static Conta Abrir(int usuarioId, decimal saldoInicial = 0m) =>
        new() { UsuarioId = usuarioId, Saldo = saldoInicial };

    // A regra mora na entidade: sem saldo, a ordem não passa daqui.
    // Saldo insuficiente é esperado, então volta no Result, e não numa exceção.
    public Result Debitar(decimal valor)
    {
        if (valor > Saldo)
            return ContaErrors.SaldoInsuficiente(Saldo, valor);

        Saldo -= valor;
        return Result.Success();
    }
}

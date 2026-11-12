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
    public void Debitar(decimal valor)
    {
        if (valor > Saldo)
            throw new SaldoInsuficienteException(Saldo, valor);

        Saldo -= valor;
    }
}

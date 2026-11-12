namespace Corretora.Api.Features.Contas;

// O dinheiro disponível do usuário para comprar ativos.
public sealed class Conta
{
    private Conta()
    {
    }

    public int UsuarioId { get; private set; }

    public decimal Saldo { get; private set; }

    public static Conta Abrir(int usuarioId, decimal saldoInicial = 0m) =>
        new() { UsuarioId = usuarioId, Saldo = saldoInicial };

    public void Debitar(decimal valor)
    {
        if (valor > Saldo)
            throw new InvalidOperationException("Saldo insuficiente.");

        Saldo -= valor;
    }
}

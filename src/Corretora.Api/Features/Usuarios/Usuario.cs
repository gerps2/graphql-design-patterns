namespace Corretora.Api.Features.Usuarios;

public sealed class Usuario
{
    // O EF Core cria o objeto por aqui e preenche as propriedades depois.
    private Usuario()
    {
    }

    public int Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Telefone { get; private set; } = string.Empty;

    public string Cpf { get; private set; } = string.Empty;

    public Endereco Endereco { get; private set; } = null!;

    public static Usuario Criar(string nome, string email, string telefone, string cpf, Endereco endereco) =>
        new()
        {
            Nome = nome,
            Email = email,
            Telefone = telefone,
            Cpf = cpf,
            Endereco = endereco,
        };
}

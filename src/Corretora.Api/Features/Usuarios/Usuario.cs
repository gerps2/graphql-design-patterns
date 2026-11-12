namespace Corretora.Api.Features.Usuarios;

public record Usuario(
    int Id,
    string Nome,
    string Email,
    string Telefone,
    string Cpf,
    Endereco Endereco);

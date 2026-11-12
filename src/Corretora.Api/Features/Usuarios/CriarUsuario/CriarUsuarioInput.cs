namespace Corretora.Api.Features.Usuarios.CriarUsuario;

public sealed record CriarUsuarioInput(
    string Nome,
    string Email,
    string Telefone,
    string Cpf,
    string Rua,
    int Numero,
    string Cidade);

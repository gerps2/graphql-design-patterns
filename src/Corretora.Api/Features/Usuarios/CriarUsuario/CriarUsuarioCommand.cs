using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Usuarios.CriarUsuario;

public sealed record CriarUsuarioCommand(
    string Nome,
    string Email,
    string Telefone,
    string Cpf,
    string Rua,
    int Numero,
    string Cidade) : ICommand<CriarUsuarioOutput>;

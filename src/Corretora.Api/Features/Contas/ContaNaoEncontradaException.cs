namespace Corretora.Api.Features.Contas;

public sealed class ContaNaoEncontradaException(int usuarioId)
    : Exception($"O usuário {usuarioId} não tem conta.")
{
    public int UsuarioId { get; } = usuarioId;
}

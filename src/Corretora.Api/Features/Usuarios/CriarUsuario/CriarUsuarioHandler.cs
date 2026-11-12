using Corretora.Api.Common.Data;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;
using Corretora.Api.Features.Contas;
using Microsoft.EntityFrameworkCore;

namespace Corretora.Api.Features.Usuarios.CriarUsuario;

public sealed class CriarUsuarioHandler(
    IUsuarioRepository usuarios,
    IContaRepository contas,
    IUnitOfWork unitOfWork) : ICommandHandler<CriarUsuarioCommand, CriarUsuarioOutput>
{
    public async Task<CriarUsuarioOutput> Handle(CriarUsuarioCommand command, CancellationToken ct)
    {
        // A checagem cobre o caso comum. Dois cadastros ao mesmo tempo passam por ela, e quem segura é o índice único.
        if (await usuarios.ExisteEmailAsync(command.Email, ct))
            throw new EmailJaCadastradoException(command.Email);

        var usuario = Usuario.Criar(command.Nome, command.Email, command.Telefone, command.Cpf,
            new Endereco(command.Rua, command.Numero, command.Cidade));

        // Dois SaveChanges: o id do usuário só existe depois do primeiro. A transação junta os dois.
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        usuarios.Adicionar(usuario);
        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            throw new EmailJaCadastradoException(command.Email);
        }

        contas.Adicionar(Conta.Abrir(usuario.Id));
        await unitOfWork.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);
        return new CriarUsuarioOutput(UsuarioOutput.De(usuario));
    }
}

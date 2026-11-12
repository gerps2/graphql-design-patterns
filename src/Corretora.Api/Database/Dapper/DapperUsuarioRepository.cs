using Corretora.Api.Features.Usuarios;
using Dapper;

namespace Corretora.Api.Database.Dapper;

public sealed class DapperUsuarioRepository(DapperUnitOfWork unitOfWork) : IUsuarioRepository
{
    public async Task<bool> ExisteEmailAsync(string email, CancellationToken ct)
    {
        var connection = await unitOfWork.ConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition("SELECT EXISTS (SELECT 1 FROM usuarios WHERE email = @email)",
                new { email }, unitOfWork.Transaction, cancellationToken: ct));
    }

    public void Adicionar(Usuario usuario) =>
        unitOfWork.Registrar(usuario, async (connection, transaction) =>
        {
            var id = await connection.ExecuteScalarAsync<int>(
                """
                INSERT INTO usuarios (nome, email, telefone, cpf, endereco_rua, endereco_numero, endereco_cidade)
                VALUES (@Nome, @Email, @Telefone, @Cpf, @Rua, @Numero, @Cidade)
                RETURNING id
                """,
                new
                {
                    usuario.Nome,
                    usuario.Email,
                    usuario.Telefone,
                    usuario.Cpf,
                    usuario.Endereco.Rua,
                    usuario.Endereco.Numero,
                    usuario.Endereco.Cidade,
                },
                transaction);
            Identidade.Definir(usuario, id);
        });
}

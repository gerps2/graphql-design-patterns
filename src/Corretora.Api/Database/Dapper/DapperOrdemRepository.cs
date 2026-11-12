using Corretora.Api.Features.Ordens;
using Dapper;

namespace Corretora.Api.Database.Dapper;

public sealed class DapperOrdemRepository(DapperUnitOfWork unitOfWork) : IOrdemRepository
{
    public void Adicionar(Ordem ordem) =>
        unitOfWork.Registrar(ordem, async (connection, transaction) =>
        {
            var id = await connection.ExecuteScalarAsync<int>(
                """
                INSERT INTO ordens (usuario_id, ativo, quantidade, preco, criada_em)
                VALUES (@UsuarioId, @Ativo, @Quantidade, @Preco, @CriadaEm)
                RETURNING id
                """,
                ordem,
                transaction);
            Identidade.Definir(ordem, id);
        });
}

using Corretora.Api.Features.Ordens;

namespace Corretora.Api.Database.Repositories;

public sealed class OrdemRepository(CorretoraDbContext db) : IOrdemRepository
{
    public void Adicionar(Ordem ordem) => db.Ordens.Add(ordem);
}

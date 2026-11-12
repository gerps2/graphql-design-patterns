using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Corretora.Api.Database;

// O EF Core monta o modelo uma vez e guarda. Sem trava, o modelo não tem o token de versão,
// então cada estratégia precisa da sua cópia guardada.
public sealed class ModeloPorEstrategia : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime) =>
        context is CorretoraDbContext db
            ? (context.GetType(), db.Estrategia, designTime)
            : (context.GetType(), designTime);
}

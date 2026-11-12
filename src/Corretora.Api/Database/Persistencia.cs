namespace Corretora.Api.Database;

// Quem grava: o EF Core (padrão) ou o Dapper. Escolhido em Persistencia:Provedor.
public enum ProvedorDePersistencia
{
    EfCore,
    Dapper,
}

public sealed class PersistenciaOptions
{
    public ProvedorDePersistencia Provedor { get; set; } = ProvedorDePersistencia.EfCore;
}

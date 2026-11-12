namespace Corretora.Api.Database.Dapper;

// O id nasce no banco e volta para a entidade, que só tem setter privado. O EF Core faz o mesmo por reflexão.
internal static class Identidade
{
    public static void Definir(object entidade, object id) =>
        entidade.GetType().GetProperty("Id")!.SetValue(entidade, id);
}

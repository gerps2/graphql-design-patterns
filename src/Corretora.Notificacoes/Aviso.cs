namespace Corretora.Notificacoes;

// Um aviso para o investidor: "a sua ordem foi enviada", "bem-vindo". É o dado deste serviço.
public sealed record Aviso(int UsuarioId, string Texto, DateTimeOffset CriadoEm);

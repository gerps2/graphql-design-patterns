using Corretora.Api.Common.Data;
using Corretora.Api.Features.Contas;
using Corretora.Api.Features.Usuarios;
using Corretora.Api.Features.Usuarios.CriarUsuario;

namespace Corretora.UnitTests.Features.Usuarios.CriarUsuario;

public class CriarUsuarioHandlerTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IContaRepository _contas = Substitute.For<IContaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITransaction _transaction = Substitute.For<ITransaction>();
    private readonly CriarUsuarioHandler _handler;

    public CriarUsuarioHandlerTests()
    {
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);
        _handler = new CriarUsuarioHandler(_usuarios, _contas, _unitOfWork);
    }

    private static readonly CriarUsuarioCommand Command =
        new("Fábio Reis", "fabio@corretora.com", "11 99999-0006", "222.333.444-55", "Rua Nova", 1, "Recife");

    [Fact]
    public async Task Handle_EmailNovo_CriaUsuarioEContaNaMesmaTransacao()
    {
        // Arrange
        _usuarios.ExisteEmailAsync(Command.Email, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var output = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        output.Usuario!.Nome.Should().Be("Fábio Reis");
        _usuarios.Received(1).Adicionar(Arg.Is<Usuario>(u => u.Email == Command.Email));
        _contas.Received(1).Adicionar(Arg.Is<Conta>(c => c.Saldo == 0m));
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailJaCadastrado_LancaSemSalvar()
    {
        // Arrange
        _usuarios.ExisteEmailAsync(Command.Email, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var criar = () => _handler.Handle(Command, CancellationToken.None);

        // Assert
        await criar.Should().ThrowAsync<EmailJaCadastradoException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailGravadoPorOutroAoMesmoTempo_LancaEmailJaCadastradoSemCommit()
    {
        // Arrange
        _usuarios.ExisteEmailAsync(Command.Email, Arg.Any<CancellationToken>()).Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns<int>(_ => throw new UniqueConstraintViolationException(new Exception("ix_usuarios_email")));

        // Act
        var criar = () => _handler.Handle(Command, CancellationToken.None);

        // Assert
        await criar.Should().ThrowAsync<EmailJaCadastradoException>();
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}

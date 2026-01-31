using AgroSolutions.Properties.Application.Commands.Produtores;
using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AgroSolutions.Properties.Tests.Application.Commands;

public class CreateProdutorCommandHandlerTests
{
    private readonly Mock<IProdutorRepository> _repositoryMock;
    private readonly CreateProdutorCommandHandler _handler;

    public CreateProdutorCommandHandlerTests()
    {
        _repositoryMock = new Mock<IProdutorRepository>();
        _handler = new CreateProdutorCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProdutor()
    {
        // Arrange
        var command = new CreateProdutorCommand
        {
            Nome = "João Silva",
            Cpf = "12345678901",
            Email = "joao@example.com",
            Telefone = "11999999999",
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Produtor>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Produtor p, CancellationToken _) => p);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _repositoryMock.Verify(
            r =>
                r.AddAsync(
                    It.Is<Produtor>(p =>
                        p.Nome == command.Nome && p.Cpf == command.Cpf && p.IsActive == true
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}

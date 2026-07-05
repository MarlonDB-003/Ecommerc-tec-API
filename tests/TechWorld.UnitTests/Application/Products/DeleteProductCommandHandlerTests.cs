using FluentAssertions;
using Moq;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Products.Commands.DeleteProduct;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Products;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _handler = new DeleteProductCommandHandler(
            _productRepo.Object, _unitOfWork.Object, _currentUser.Object);
    }

    [Fact]
    public async Task Handle_WhenNotAdmin_ThrowsForbiddenException()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(false);
        var act = async () => await _handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ThrowsNotFoundException()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        _productRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var act = async () => await _handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenValid_RemovesProductAndSaves()
    {
        var product = Product.Create("Produto para deletar", 100m, "Gaming");
        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        await _handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        _productRepo.Verify(r => r.Remove(product), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonAdmin_DoesNotRemoveProduct()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(false);
        var act = async () => await _handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
        _productRepo.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

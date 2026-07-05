using FluentAssertions;
using Moq;
using TechWorld.Application.Addresses.Commands.DeleteAddress;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Addresses;

public class DeleteAddressCommandHandlerTests
{
    private readonly Mock<IAddressRepository> _addressRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteAddressCommandHandler _handler;

    public DeleteAddressCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _handler = new DeleteAddressCommandHandler(_addressRepo.Object, _currentUser.Object, _unitOfWork.Object);
    }

    private static Address CreateAddress(Guid userId, bool isDefault = false) =>
        Address.Create(userId, "Casa", "01310-100", "Av. Paulista", "100",
            null, "Bela Vista", "São Paulo", "SP", isDefault);

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsForbiddenException()
    {
        _currentUser.Setup(u => u.UserId).Returns((Guid?)null);

        var act = async () => await _handler.Handle(new DeleteAddressCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenAddressNotFound_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _addressRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Address?)null);

        var act = async () => await _handler.Handle(new DeleteAddressCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAddressBelongsToDifferentUser_ThrowsForbiddenException()
    {
        var requesterId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var address = CreateAddress(ownerId);

        _currentUser.Setup(u => u.UserId).Returns(requesterId);
        _addressRepo
            .Setup(r => r.GetByIdAsync(address.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        var act = async () => await _handler.Handle(new DeleteAddressCommand(address.Id), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenValid_DeletesAddressAndSaves()
    {
        var userId = Guid.NewGuid();
        var address = CreateAddress(userId, isDefault: false);

        _currentUser.Setup(u => u.UserId).Returns(userId);
        _addressRepo
            .Setup(r => r.GetByIdAsync(address.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);
        _addressRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _handler.Handle(new DeleteAddressCommand(address.Id), CancellationToken.None);

        _addressRepo.Verify(r => r.Delete(address), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WhenDeletingDefaultAddress_PromotesNextAsDefault()
    {
        var userId = Guid.NewGuid();
        var defaultAddr = CreateAddress(userId, isDefault: true);
        var nextAddr = CreateAddress(userId, isDefault: false);

        _currentUser.Setup(u => u.UserId).Returns(userId);
        _addressRepo
            .Setup(r => r.GetByIdAsync(defaultAddr.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultAddr);
        _addressRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([nextAddr]);

        await _handler.Handle(new DeleteAddressCommand(defaultAddr.Id), CancellationToken.None);

        nextAddr.IsDefault.Should().BeTrue();
        _addressRepo.Verify(r => r.Update(nextAddr), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDeletingNonDefaultAddress_DoesNotPromoteAnother()
    {
        var userId = Guid.NewGuid();
        var nonDefault = CreateAddress(userId, isDefault: false);

        _currentUser.Setup(u => u.UserId).Returns(userId);
        _addressRepo
            .Setup(r => r.GetByIdAsync(nonDefault.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nonDefault);

        await _handler.Handle(new DeleteAddressCommand(nonDefault.Id), CancellationToken.None);

        _addressRepo.Verify(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _addressRepo.Verify(r => r.Update(It.IsAny<Address>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDeletingLastDefaultAddress_DoesNotCallUpdate()
    {
        var userId = Guid.NewGuid();
        var defaultAddr = CreateAddress(userId, isDefault: true);

        _currentUser.Setup(u => u.UserId).Returns(userId);
        _addressRepo
            .Setup(r => r.GetByIdAsync(defaultAddr.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultAddr);
        _addressRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]); // no remaining addresses

        await _handler.Handle(new DeleteAddressCommand(defaultAddr.Id), CancellationToken.None);

        _addressRepo.Verify(r => r.Update(It.IsAny<Address>()), Times.Never);
    }
}

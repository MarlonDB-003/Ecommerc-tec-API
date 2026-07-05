using FluentAssertions;
using Moq;
using TechWorld.Application.Addresses.Commands.SetDefaultAddress;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Addresses;

public class SetDefaultAddressCommandHandlerTests
{
    private readonly Mock<IAddressRepository> _addressRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly SetDefaultAddressCommandHandler _handler;

    public SetDefaultAddressCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _handler = new SetDefaultAddressCommandHandler(_addressRepo.Object, _currentUser.Object, _unitOfWork.Object);
    }

    private static Address CreateAddress(Guid userId, bool isDefault = false) =>
        Address.Create(userId, "Casa", "01310-100", "Av. Paulista", "100",
            null, "Bela Vista", "São Paulo", "SP", isDefault);

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsForbiddenException()
    {
        _currentUser.Setup(u => u.UserId).Returns((Guid?)null);

        var act = async () => await _handler.Handle(new SetDefaultAddressCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenAddressNotInUserList_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _addressRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var act = async () => await _handler.Handle(new SetDefaultAddressCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenValid_SetsTargetAsDefault()
    {
        var userId = Guid.NewGuid();
        var target = CreateAddress(userId, isDefault: false);
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _addressRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([target]);

        await _handler.Handle(new SetDefaultAddressCommand(target.Id), CancellationToken.None);

        target.IsDefault.Should().BeTrue();
        _addressRepo.Verify(r => r.Update(target), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOtherAddressIsDefault_UnsetsOldDefault()
    {
        var userId = Guid.NewGuid();
        var oldDefault = CreateAddress(userId, isDefault: true);
        var newDefault = CreateAddress(userId, isDefault: false);

        _currentUser.Setup(u => u.UserId).Returns(userId);
        _addressRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([oldDefault, newDefault]);

        await _handler.Handle(new SetDefaultAddressCommand(newDefault.Id), CancellationToken.None);

        oldDefault.IsDefault.Should().BeFalse();
        newDefault.IsDefault.Should().BeTrue();
        _addressRepo.Verify(r => r.Update(oldDefault), Times.Once);
        _addressRepo.Verify(r => r.Update(newDefault), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTargetAlreadyDefault_DoesNotUnsetAndStillSaves()
    {
        var userId = Guid.NewGuid();
        var target = CreateAddress(userId, isDefault: true);

        _currentUser.Setup(u => u.UserId).Returns(userId);
        _addressRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([target]);

        await _handler.Handle(new SetDefaultAddressCommand(target.Id), CancellationToken.None);

        target.IsDefault.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

using FluentAssertions;
using Moq;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Users.Commands.UpdateProfile;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Users;

public class UpdateProfileCommandHandlerTests
{
    private readonly Mock<IUserProfileRepository> _profileRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly UpdateProfileCommandHandler _handler;

    public UpdateProfileCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _handler = new UpdateProfileCommandHandler(_profileRepo.Object, _unitOfWork.Object, _currentUser.Object);
    }

    private static UpdateProfileCommand ValidCommand() =>
        new("João Silva Atualizado", "11988887777", null);

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsForbiddenException()
    {
        _currentUser.Setup(u => u.UserId).Returns((Guid?)null);

        var act = async () => await _handler.Handle(ValidCommand(), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_CreatesNewProfile()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _currentUser.Setup(u => u.Email).Returns("joao@ex.com");
        _currentUser.Setup(u => u.IsAdmin).Returns(false);
        _profileRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        _profileRepo.Verify(r => r.AddAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProfileExists_UpdatesExistingProfile()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _currentUser.Setup(u => u.Email).Returns("joao@ex.com");
        _currentUser.Setup(u => u.IsAdmin).Returns(false);

        var existingProfile = UserProfile.Create(userId, "Nome Antigo");
        _profileRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProfile);

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        _profileRepo.Verify(r => r.Update(existingProfile), Times.Once);
        _profileRepo.Verify(r => r.AddAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProfileExists_ReturnsDtoWithUpdatedData()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _currentUser.Setup(u => u.Email).Returns("joao@ex.com");
        _currentUser.Setup(u => u.IsAdmin).Returns(false);

        var existingProfile = UserProfile.Create(userId, "Nome Antigo");
        _profileRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProfile);

        var cmd = new UpdateProfileCommand("João Novo", "11999999999", "https://img.ex.com/av.jpg");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.DisplayName.Should().Be("João Novo");
        result.Phone.Should().Be("11999999999");
        result.AvatarUrl.Should().Be("https://img.ex.com/av.jpg");
        result.UserId.Should().Be(userId);
        result.Email.Should().Be("joao@ex.com");
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_DoesNotCallUpdate()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _currentUser.Setup(u => u.Email).Returns("joao@ex.com");
        _currentUser.Setup(u => u.IsAdmin).Returns(false);
        _profileRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        _profileRepo.Verify(r => r.Update(It.IsAny<UserProfile>()), Times.Never);
    }
}

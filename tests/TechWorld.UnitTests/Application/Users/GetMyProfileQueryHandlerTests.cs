using FluentAssertions;
using Moq;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Users.Queries.GetMyProfile;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Users;

public class GetMyProfileQueryHandlerTests
{
    private readonly Mock<IUserProfileRepository> _profileRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly GetMyProfileQueryHandler _handler;

    public GetMyProfileQueryHandlerTests()
    {
        _handler = new GetMyProfileQueryHandler(_profileRepo.Object, _currentUser.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsForbiddenException()
    {
        _currentUser.Setup(u => u.UserId).Returns((Guid?)null);

        var act = async () => await _handler.Handle(new GetMyProfileQuery(), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenProfileExists_ReturnsDtoWithProfileData()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _currentUser.Setup(u => u.Email).Returns("joao@ex.com");
        _currentUser.Setup(u => u.IsAdmin).Returns(false);

        var profile = UserProfile.Create(userId, "João Silva");
        profile.Update("João Silva", "11999999999", "https://img.example.com/avatar.jpg");
        _profileRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _handler.Handle(new GetMyProfileQuery(), CancellationToken.None);

        result.UserId.Should().Be(userId);
        result.Email.Should().Be("joao@ex.com");
        result.DisplayName.Should().Be("João Silva");
        result.Phone.Should().Be("11999999999");
        result.AvatarUrl.Should().Be("https://img.example.com/avatar.jpg");
        result.IsAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsDtoWithNullFields()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _currentUser.Setup(u => u.Email).Returns("novo@ex.com");
        _currentUser.Setup(u => u.IsAdmin).Returns(false);

        _profileRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        var result = await _handler.Handle(new GetMyProfileQuery(), CancellationToken.None);

        result.UserId.Should().Be(userId);
        result.Email.Should().Be("novo@ex.com");
        result.DisplayName.Should().BeNull();
        result.Phone.Should().BeNull();
        result.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_ReturnsDtoWithIsAdminTrue()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _currentUser.Setup(u => u.Email).Returns("admin@ex.com");
        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        _profileRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        var result = await _handler.Handle(new GetMyProfileQuery(), CancellationToken.None);

        result.IsAdmin.Should().BeTrue();
    }
}

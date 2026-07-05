using FluentAssertions;
using TechWorld.Domain.Entities;

namespace TechWorld.UnitTests.Domain;

public class UserProfileTests
{
    [Fact]
    public void Create_SetsUserIdAndDisplayName()
    {
        var userId = Guid.NewGuid();
        var profile = UserProfile.Create(userId, "João Silva");

        profile.UserId.Should().Be(userId);
        profile.DisplayName.Should().Be("João Silva");
        profile.Phone.Should().BeNull();
        profile.AvatarUrl.Should().BeNull();
        profile.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithNullDisplayName_LeavesDisplayNameNull()
    {
        var profile = UserProfile.Create(Guid.NewGuid(), null);
        profile.DisplayName.Should().BeNull();
    }

    [Fact]
    public void Update_SetsAllFieldsCorrectly()
    {
        var profile = UserProfile.Create(Guid.NewGuid(), "Nome Antigo");
        profile.Update("Nome Novo", "11999999999", "https://cdn.example.com/avatar.jpg");

        profile.DisplayName.Should().Be("Nome Novo");
        profile.Phone.Should().Be("11999999999");
        profile.AvatarUrl.Should().Be("https://cdn.example.com/avatar.jpg");
    }

    [Fact]
    public void Update_WithNullValues_ClearsAllFields()
    {
        var profile = UserProfile.Create(Guid.NewGuid(), "João");
        profile.Update("João", "11999999999", "https://img.example.com/avatar.jpg");
        profile.Update(null, null, null);

        profile.DisplayName.Should().BeNull();
        profile.Phone.Should().BeNull();
        profile.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public void Update_PartialFields_OnlyChangesProvided()
    {
        var profile = UserProfile.Create(Guid.NewGuid(), "João");
        profile.Update("Maria", null, null);

        profile.DisplayName.Should().Be("Maria");
        profile.Phone.Should().BeNull();
        profile.AvatarUrl.Should().BeNull();
    }
}

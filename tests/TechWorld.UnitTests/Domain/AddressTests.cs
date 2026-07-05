using FluentAssertions;
using TechWorld.Domain.Entities;

namespace TechWorld.UnitTests.Domain;

public class AddressTests
{
    private static Address CreateAddress(bool isDefault = false, Guid? userId = null)
        => Address.Create(
            userId ?? Guid.NewGuid(), "Casa",
            "01310-100", "Av. Paulista", "1000",
            null, "Bela Vista", "São Paulo", "SP", isDefault);

    [Fact]
    public void Create_SetsAllPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();
        var address = Address.Create(userId, "Trabalho", "01310-100",
            "Av. Paulista", "100", "Sala 5",
            "Bela Vista", "São Paulo", "SP");

        address.UserId.Should().Be(userId);
        address.Label.Should().Be("Trabalho");
        address.Cep.Should().Be("01310-100");
        address.Street.Should().Be("Av. Paulista");
        address.Number.Should().Be("100");
        address.Complement.Should().Be("Sala 5");
        address.Neighborhood.Should().Be("Bela Vista");
        address.City.Should().Be("São Paulo");
        address.State.Should().Be("SP");
        address.IsDefault.Should().BeFalse();
        address.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_TrimsLabel()
    {
        var address = Address.Create(Guid.NewGuid(), "  Trabalho  ",
            "01310-100", "Av. Paulista", "100",
            null, "Centro", "São Paulo", "SP");

        address.Label.Should().Be("Trabalho");
    }

    [Fact]
    public void Create_WithNullLabel_LeavesLabelNull()
    {
        var address = Address.Create(Guid.NewGuid(), null,
            "01310-100", "Av. Paulista", "100",
            null, "Centro", "São Paulo", "SP");

        address.Label.Should().BeNull();
    }

    [Fact]
    public void Create_WithIsDefaultTrue_SetsIsDefaultTrue()
    {
        var address = CreateAddress(isDefault: true);
        address.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void SetAsDefault_SetsIsDefaultToTrue()
    {
        var address = CreateAddress(isDefault: false);
        address.SetAsDefault();
        address.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void UnsetDefault_SetsIsDefaultToFalse()
    {
        var address = CreateAddress(isDefault: true);
        address.UnsetDefault();
        address.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void SetAsDefault_WhenAlreadyDefault_RemainsTrue()
    {
        var address = CreateAddress(isDefault: true);
        address.SetAsDefault();
        address.IsDefault.Should().BeTrue();
    }
}

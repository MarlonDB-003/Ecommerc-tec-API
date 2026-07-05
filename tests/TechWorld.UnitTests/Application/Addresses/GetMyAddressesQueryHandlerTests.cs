using AutoMapper;
using FluentAssertions;
using Moq;
using TechWorld.Application.Addresses;
using TechWorld.Application.Addresses.Queries.GetMyAddresses;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Addresses;

public class GetMyAddressesQueryHandlerTests
{
    private readonly Mock<IAddressRepository> _addressRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly GetMyAddressesQueryHandler _handler;

    public GetMyAddressesQueryHandlerTests()
    {
        _handler = new GetMyAddressesQueryHandler(_addressRepo.Object, _currentUser.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsForbiddenException()
    {
        _currentUser.Setup(u => u.UserId).Returns((Guid?)null);

        var act = async () => await _handler.Handle(new GetMyAddressesQuery(), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenAuthenticated_ReturnsMappedAddresses()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);

        var addresses = new List<Address>
        {
            Address.Create(userId, "Casa", "01310-100", "Av. Paulista", "100", null, "Bela Vista", "SP", "SP", true),
            Address.Create(userId, "Trabalho", "01310-100", "Rua Augusta", "50", null, "Centro", "SP", "SP"),
        };
        _addressRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(addresses);

        var fakeDtos = new List<AddressDto>
        {
            new(Guid.NewGuid(), userId, "Casa", "01310-100", "Av. Paulista", "100", null, "Bela Vista", "SP", "SP", true, DateTime.UtcNow),
            new(Guid.NewGuid(), userId, "Trabalho", "01310-100", "Rua Augusta", "50", null, "Centro", "SP", "SP", false, DateTime.UtcNow),
        };
        _mapper
            .Setup(m => m.Map<IEnumerable<AddressDto>>(It.IsAny<IEnumerable<Address>>()))
            .Returns(fakeDtos);

        var result = await _handler.Handle(new GetMyAddressesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoAddresses_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _addressRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _mapper
            .Setup(m => m.Map<IEnumerable<AddressDto>>(It.IsAny<IEnumerable<Address>>()))
            .Returns([]);

        var result = await _handler.Handle(new GetMyAddressesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PassesCorrectUserIdToRepository()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _addressRepo
            .Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _mapper
            .Setup(m => m.Map<IEnumerable<AddressDto>>(It.IsAny<IEnumerable<Address>>()))
            .Returns([]);

        await _handler.Handle(new GetMyAddressesQuery(), CancellationToken.None);

        _addressRepo.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

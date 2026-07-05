using AutoMapper;
using FluentAssertions;
using Moq;
using TechWorld.Application.Addresses;
using TechWorld.Application.Addresses.Commands.CreateAddress;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Addresses;

public class CreateAddressCommandHandlerTests
{
    private readonly Mock<IAddressRepository> _addressRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly CreateAddressCommandHandler _handler;

    public CreateAddressCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _handler = new CreateAddressCommandHandler(
            _addressRepo.Object, _currentUser.Object, _unitOfWork.Object, _mapper.Object);
    }

    private static CreateAddressCommand ValidCommand() => new(
        "Casa", "01310-100", "Av. Paulista", "1000",
        null, "Bela Vista", "São Paulo", "SP");

    private static AddressDto FakeDto(Guid id) => new(
        id, Guid.NewGuid(), "Casa", "01310-100", "Av. Paulista",
        "1000", null, "Bela Vista", "São Paulo", "SP", false, DateTime.UtcNow);

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsForbiddenException()
    {
        _currentUser.Setup(u => u.UserId).Returns((Guid?)null);

        var act = async () => await _handler.Handle(ValidCommand(), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenFirstAddress_SetsIsDefaultTrue()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _addressRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        Address? capturedAddress = null;
        _addressRepo
            .Setup(r => r.AddAsync(It.IsAny<Address>(), It.IsAny<CancellationToken>()))
            .Callback<Address, CancellationToken>((a, _) => capturedAddress = a)
            .Returns(Task.CompletedTask);
        _mapper.Setup(m => m.Map<AddressDto>(It.IsAny<Address>())).Returns(FakeDto(Guid.NewGuid()));

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        capturedAddress.Should().NotBeNull();
        capturedAddress!.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenNotFirstAddress_SetsIsDefaultFalse()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);

        var existing = new List<Address>
        {
            Address.Create(userId, "Trabalho", "01310-100", "Av. Paulista", "100", null, "Bela Vista", "SP", "SP", true)
        };
        _addressRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        Address? capturedAddress = null;
        _addressRepo
            .Setup(r => r.AddAsync(It.IsAny<Address>(), It.IsAny<CancellationToken>()))
            .Callback<Address, CancellationToken>((a, _) => capturedAddress = a)
            .Returns(Task.CompletedTask);
        _mapper.Setup(m => m.Map<AddressDto>(It.IsAny<Address>())).Returns(FakeDto(Guid.NewGuid()));

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        capturedAddress!.IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenValid_SavesAndReturnsMappedDto()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _addressRepo
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _mapper.Setup(m => m.Map<AddressDto>(It.IsAny<Address>())).Returns(FakeDto(Guid.NewGuid()));

        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        _addressRepo.Verify(r => r.AddAsync(It.IsAny<Address>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.Should().NotBeNull();
        result.Label.Should().Be("Casa");
    }
}

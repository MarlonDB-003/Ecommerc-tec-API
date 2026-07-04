using MediatR;

namespace TechWorld.Application.Addresses.Commands.SetDefaultAddress;

public record SetDefaultAddressCommand(Guid AddressId) : IRequest;

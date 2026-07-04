using MediatR;

namespace TechWorld.Application.Addresses.Commands.DeleteAddress;

public record DeleteAddressCommand(Guid AddressId) : IRequest;

using MediatR;
using TechWorld.Application.Addresses;

namespace TechWorld.Application.Addresses.Commands.CreateAddress;

public record CreateAddressCommand(
    string? Label,
    string Cep,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State
) : IRequest<AddressDto>;

using MediatR;
using TechWorld.Application.Addresses;

namespace TechWorld.Application.Addresses.Queries.GetMyAddresses;

public record GetMyAddressesQuery : IRequest<IEnumerable<AddressDto>>;

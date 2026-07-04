using AutoMapper;
using MediatR;
using TechWorld.Application.Addresses;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Addresses.Queries.GetMyAddresses;

public class GetMyAddressesQueryHandler(
    IAddressRepository addressRepository,
    ICurrentUserService currentUser,
    IMapper mapper) : IRequestHandler<GetMyAddressesQuery, IEnumerable<AddressDto>>
{
    public async Task<IEnumerable<AddressDto>> Handle(GetMyAddressesQuery request, CancellationToken ct)
    {
        if (currentUser.UserId is null) throw new ForbiddenException();

        var addresses = await addressRepository.GetByUserIdAsync(currentUser.UserId.Value, ct);
        return mapper.Map<IEnumerable<AddressDto>>(addresses);
    }
}

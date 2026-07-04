using AutoMapper;
using MediatR;
using TechWorld.Application.Addresses;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Addresses.Commands.CreateAddress;

public class CreateAddressCommandHandler(
    IAddressRepository addressRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateAddressCommand, AddressDto>
{
    public async Task<AddressDto> Handle(CreateAddressCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is null) throw new ForbiddenException();

        var existing = await addressRepository.GetByUserIdAsync(currentUser.UserId.Value, ct);
        var isFirst = !existing.Any();

        var address = Address.Create(
            currentUser.UserId.Value,
            request.Label,
            request.Cep,
            request.Street,
            request.Number,
            request.Complement,
            request.Neighborhood,
            request.City,
            request.State,
            isDefault: isFirst);

        await addressRepository.AddAsync(address, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<AddressDto>(address);
    }
}

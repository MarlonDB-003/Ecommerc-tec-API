using MediatR;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Addresses.Commands.SetDefaultAddress;

public class SetDefaultAddressCommandHandler(
    IAddressRepository addressRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<SetDefaultAddressCommand>
{
    public async Task Handle(SetDefaultAddressCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is null) throw new ForbiddenException();

        var addresses = (await addressRepository.GetByUserIdAsync(currentUser.UserId.Value, ct)).ToList();
        var target = addresses.FirstOrDefault(a => a.Id == request.AddressId)
            ?? throw new NotFoundException("Address", request.AddressId);

        foreach (var addr in addresses.Where(a => a.IsDefault && a.Id != target.Id))
        {
            addr.UnsetDefault();
            addressRepository.Update(addr);
        }

        target.SetAsDefault();
        addressRepository.Update(target);
        await unitOfWork.SaveChangesAsync(ct);
    }
}

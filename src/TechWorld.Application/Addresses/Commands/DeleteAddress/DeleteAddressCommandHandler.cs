using MediatR;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Addresses.Commands.DeleteAddress;

public class DeleteAddressCommandHandler(
    IAddressRepository addressRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteAddressCommand>
{
    public async Task Handle(DeleteAddressCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is null) throw new ForbiddenException();

        var address = await addressRepository.GetByIdAsync(request.AddressId, ct)
            ?? throw new NotFoundException("Address", request.AddressId);

        if (address.UserId != currentUser.UserId.Value) throw new ForbiddenException();

        var wasDefault = address.IsDefault;
        addressRepository.Delete(address);
        await unitOfWork.SaveChangesAsync(ct);

        if (wasDefault)
        {
            var remaining = (await addressRepository.GetByUserIdAsync(currentUser.UserId.Value, ct)).ToList();
            if (remaining.Count > 0)
            {
                remaining[0].SetAsDefault();
                addressRepository.Update(remaining[0]);
                await unitOfWork.SaveChangesAsync(ct);
            }
        }
    }
}

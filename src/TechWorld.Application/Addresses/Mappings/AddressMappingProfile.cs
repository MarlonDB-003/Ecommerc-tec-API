using AutoMapper;
using TechWorld.Domain.Entities;

namespace TechWorld.Application.Addresses.Mappings;

public class AddressMappingProfile : Profile
{
    public AddressMappingProfile()
    {
        CreateMap<Address, AddressDto>();
    }
}

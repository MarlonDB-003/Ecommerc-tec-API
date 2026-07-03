using AutoMapper;
using TechWorld.Application.Products.Queries.GetProductById;
using TechWorld.Application.Products.Queries.GetProducts;
using TechWorld.Domain.Entities;

namespace TechWorld.Application.Common.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<ProductSpecification, SpecificationDto>();

        CreateMap<Product, ProductListDto>()
            .ForCtorParam("DiscountedPrice", opt => opt.MapFrom(p => p.CalculateDiscountedPrice()));

        CreateMap<Product, ProductDetailDto>()
            .ForCtorParam("DiscountedPrice", opt => opt.MapFrom(p => p.CalculateDiscountedPrice()));
    }
}

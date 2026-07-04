namespace TechWorld.Application.Products.Queries.GetProductById;

public record ProductDetailDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    decimal DiscountedPrice,
    string? ImageUrl,
    string Category,
    string? Brand,
    int Stock,
    bool IsActive,
    int DiscountPercentage,
    DateTime CreatedAt,
    IEnumerable<SpecificationDto> Specifications
);

public record SpecificationDto(Guid Id, string Label, string Value, int DisplayOrder);

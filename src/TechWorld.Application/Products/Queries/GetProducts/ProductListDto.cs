namespace TechWorld.Application.Products.Queries.GetProducts;

public record ProductListDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    decimal DiscountedPrice,
    string? ImageUrl,
    string Category,
    int Stock,
    bool IsActive,
    int DiscountPercentage,
    DateTime CreatedAt
);

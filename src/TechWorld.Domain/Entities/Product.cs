using TechWorld.Domain.Common;
using TechWorld.Domain.Exceptions;

namespace TechWorld.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string? ImageUrl { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string? Brand { get; private set; }
    public int Stock { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DiscountPercentage { get; private set; }

    private readonly List<ProductSpecification> _specifications = [];
    public IReadOnlyCollection<ProductSpecification> Specifications => _specifications.AsReadOnly();

    protected Product() { }

    public static Product Create(
        string name,
        decimal price,
        string category,
        string? description = null,
        string? imageUrl = null,
        int stock = 0,
        int discountPercentage = 0,
        string? brand = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do produto é obrigatório.");
        if (price <= 0)
            throw new DomainException("Preço deve ser maior que zero.");
        if (discountPercentage is < 0 or > 100)
            throw new DomainException("Percentual de desconto deve estar entre 0 e 100.");

        return new Product
        {
            Name = name,
            Price = price,
            Category = category,
            Description = description,
            ImageUrl = imageUrl,
            Stock = stock,
            DiscountPercentage = discountPercentage,
            Brand = brand
        };
    }

    public void Update(
        string name,
        decimal price,
        string category,
        string? description,
        string? imageUrl,
        int stock,
        int discountPercentage,
        string? brand)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do produto é obrigatório.");
        if (price <= 0)
            throw new DomainException("Preço deve ser maior que zero.");
        if (discountPercentage is < 0 or > 100)
            throw new DomainException("Percentual de desconto deve estar entre 0 e 100.");

        Name = name;
        Price = price;
        Category = category;
        Description = description;
        ImageUrl = imageUrl;
        Stock = stock;
        DiscountPercentage = discountPercentage;
        Brand = brand;
        SetUpdatedAt();
    }

    public decimal CalculateDiscountedPrice() =>
        DiscountPercentage > 0
            ? Price * (1 - DiscountPercentage / 100m)
            : Price;

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void UpdateStock(int delta)
    {
        var newStock = Stock + delta;
        if (newStock < 0)
            throw new DomainException($"Estoque insuficiente para o produto '{Name}'.");
        Stock = newStock;
        SetUpdatedAt();
    }

    public void SetSpecifications(IEnumerable<ProductSpecification> specs)
    {
        _specifications.Clear();
        _specifications.AddRange(specs);
    }
}

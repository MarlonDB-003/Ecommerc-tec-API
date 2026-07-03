using TechWorld.Domain.Common;

namespace TechWorld.Domain.Entities;

public class ProductSpecification : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    public Product? Product { get; private set; }

    protected ProductSpecification() { }

    public static ProductSpecification Create(Guid productId, string label, string value, int displayOrder = 0) =>
        new()
        {
            ProductId = productId,
            Label = label,
            Value = value,
            DisplayOrder = displayOrder
        };
}

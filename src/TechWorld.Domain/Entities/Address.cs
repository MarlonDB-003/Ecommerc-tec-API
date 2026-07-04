using TechWorld.Domain.Common;

namespace TechWorld.Domain.Entities;

public class Address : BaseEntity
{
    public Guid UserId { get; private set; }
    public string? Label { get; private set; }
    public string Cep { get; private set; } = string.Empty;
    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string? Complement { get; private set; }
    public string Neighborhood { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }

    protected Address() { }

    public static Address Create(
        Guid userId,
        string? label,
        string cep,
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state,
        bool isDefault = false)
    {
        return new Address
        {
            UserId = userId,
            Label = label?.Trim(),
            Cep = cep,
            Street = street,
            Number = number,
            Complement = complement,
            Neighborhood = neighborhood,
            City = city,
            State = state,
            IsDefault = isDefault,
        };
    }

    public void SetAsDefault() { IsDefault = true; SetUpdatedAt(); }
    public void UnsetDefault() { IsDefault = false; SetUpdatedAt(); }
}

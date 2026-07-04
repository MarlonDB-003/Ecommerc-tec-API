namespace TechWorld.Application.Addresses;

public record AddressDto(
    Guid Id,
    Guid UserId,
    string? Label,
    string Cep,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    bool IsDefault,
    DateTime CreatedAt
);

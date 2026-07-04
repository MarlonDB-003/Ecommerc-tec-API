using TechWorld.Domain.Entities;

namespace TechWorld.Domain.Interfaces.Repositories;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Address address, CancellationToken ct = default);
    void Update(Address address);
    void Delete(Address address);
}

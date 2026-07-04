using Microsoft.EntityFrameworkCore;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Infrastructure.Persistence.Repositories;

public class AddressRepository(ApplicationDbContext db) : IAddressRepository
{
    public async Task<Address?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Addresses.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await db.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Address address, CancellationToken ct = default) =>
        await db.Addresses.AddAsync(address, ct);

    public void Update(Address address) => db.Addresses.Update(address);

    public void Delete(Address address) => db.Addresses.Remove(address);
}

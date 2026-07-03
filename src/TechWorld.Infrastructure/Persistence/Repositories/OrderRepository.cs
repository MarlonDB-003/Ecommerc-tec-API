using Microsoft.EntityFrameworkCore;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Infrastructure.Persistence.Repositories;

public class OrderRepository(ApplicationDbContext db) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default) =>
        await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await db.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Orders.Include(o => o.Items).OrderByDescending(o => o.CreatedAt);
        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, totalCount);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default) =>
        await db.Orders.AddAsync(order, ct);

    public void Update(Order order) => db.Orders.Update(order);
}

using Microsoft.EntityFrameworkCore;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Infrastructure.Persistence.Repositories;

public class ProductRepository(ApplicationDbContext db) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Product?> GetByIdWithSpecificationsAsync(Guid id, CancellationToken ct = default) =>
        await db.Products
            .Include(p => p.Specifications.OrderBy(s => s.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        string? category,
        string? search,
        bool? isActive,
        string sortBy,
        bool ascending,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) && category.ToLower() != "todos")
            query = query.Where(p => p.Category.ToLower() == category.ToLower());

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()) ||
                                     (p.Description != null && p.Description.ToLower().Contains(search.ToLower())));

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        query = sortBy.ToLower() switch
        {
            "price" => ascending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
            "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            "discount" => ascending ? query.OrderBy(p => p.DiscountPercentage) : query.OrderByDescending(p => p.DiscountPercentage),
            _ => ascending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IEnumerable<Product>> GetFeaturedAsync(string type, int limit, CancellationToken ct = default)
    {
        var query = db.Products.Where(p => p.IsActive);

        query = type.ToLower() switch
        {
            "discounts" => query.Where(p => p.DiscountPercentage > 0).OrderByDescending(p => p.DiscountPercentage),
            "gaming" => query.Where(p => p.Category.ToLower() == "gaming").OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        return await query.Take(limit).ToListAsync(ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default) =>
        await db.Products.AddAsync(product, ct);

    public void Update(Product product) => db.Products.Update(product);

    public void Remove(Product product) => db.Products.Remove(product);
}

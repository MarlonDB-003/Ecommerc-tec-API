using TechWorld.Domain.Entities;

namespace TechWorld.Domain.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetByIdWithSpecificationsAsync(Guid id, CancellationToken ct = default);
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        string? category,
        string? search,
        bool? isActive,
        string sortBy,
        bool ascending,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task<IEnumerable<Product>> GetFeaturedAsync(string type, int limit, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Update(Product product);
    void Remove(Product product);
}

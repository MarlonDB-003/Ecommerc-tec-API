using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechWorld.Domain.Entities;

namespace TechWorld.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.TotalAmount).HasPrecision(10, 2).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().IsRequired();
        builder.Property(o => o.PaymentMethod).HasConversion<string>().IsRequired();
        builder.Property(o => o.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(200);
        builder.Property(o => o.CustomerPhone).IsRequired().HasMaxLength(20);
        builder.Property(o => o.AddressCep).IsRequired().HasMaxLength(10);
        builder.Property(o => o.AddressStreet).IsRequired().HasMaxLength(300);
        builder.Property(o => o.AddressNumber).IsRequired().HasMaxLength(20);
        builder.Property(o => o.AddressComplement).HasMaxLength(100);
        builder.Property(o => o.AddressNeighborhood).IsRequired().HasMaxLength(100);
        builder.Property(o => o.AddressCity).IsRequired().HasMaxLength(100);
        builder.Property(o => o.AddressState).IsRequired().HasMaxLength(2);

        builder.HasIndex(o => o.UserId);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

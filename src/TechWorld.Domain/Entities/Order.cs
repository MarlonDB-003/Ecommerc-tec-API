using TechWorld.Domain.Common;
using TechWorld.Domain.Enums;
using TechWorld.Domain.Exceptions;

namespace TechWorld.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public PaymentMethod PaymentMethod { get; private set; }
    public int Installments { get; private set; } = 1;

    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;

    public string AddressCep { get; private set; } = string.Empty;
    public string AddressStreet { get; private set; } = string.Empty;
    public string AddressNumber { get; private set; } = string.Empty;
    public string? AddressComplement { get; private set; }
    public string AddressNeighborhood { get; private set; } = string.Empty;
    public string AddressCity { get; private set; } = string.Empty;
    public string AddressState { get; private set; } = string.Empty;

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    protected Order() { }

    public static Order Create(
        Guid userId,
        PaymentMethod paymentMethod,
        int installments,
        string customerName,
        string customerEmail,
        string customerPhone,
        string cep,
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state)
    {
        return new Order
        {
            UserId = userId,
            PaymentMethod = paymentMethod,
            Installments = paymentMethod == PaymentMethod.CreditCard ? installments : 1,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CustomerPhone = customerPhone,
            AddressCep = cep,
            AddressStreet = street,
            AddressNumber = number,
            AddressComplement = complement,
            AddressNeighborhood = neighborhood,
            AddressCity = city,
            AddressState = state
        };
    }

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        var item = OrderItem.Create(Id, productId, productName, quantity, unitPrice);
        _items.Add(item);
        RecalculateTotal();
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if (Status == OrderStatus.Cancelled)
            throw new DomainException("Pedido cancelado não pode ter status alterado.");

        Status = newStatus;
        SetUpdatedAt();
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new DomainException("Pedido entregue não pode ser cancelado.");

        Status = OrderStatus.Cancelled;
        SetUpdatedAt();
    }

    private void RecalculateTotal() =>
        TotalAmount = _items.Sum(i => i.Quantity * i.UnitPrice);
}

using FluentAssertions;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Enums;
using TechWorld.Domain.Exceptions;

namespace TechWorld.UnitTests.Domain;

public class OrderTests
{
    private static Order CreateOrder(PaymentMethod method = PaymentMethod.CreditCard, int installments = 1)
        => Order.Create(
            Guid.NewGuid(), method, installments,
            "João Silva", "joao@exemplo.com", "11999999999",
            "01310-100", "Av. Paulista", "1000", null,
            "Bela Vista", "São Paulo", "SP");

    [Fact]
    public void Create_WithCreditCard_PreservesInstallments()
    {
        var order = CreateOrder(PaymentMethod.CreditCard, 6);
        order.Installments.Should().Be(6);
        order.PaymentMethod.Should().Be(PaymentMethod.CreditCard);
    }

    [Theory]
    [InlineData(PaymentMethod.Pix)]
    [InlineData(PaymentMethod.Boleto)]
    [InlineData(PaymentMethod.DebitCard)]
    public void Create_WithNonCreditCard_ForcesInstallmentsToOne(PaymentMethod method)
    {
        var order = CreateOrder(method, 6);
        order.Installments.Should().Be(1);
    }

    [Fact]
    public void Create_SetsInitialStatusToPending()
    {
        var order = CreateOrder();
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Create_SetsInitialTotalToZero()
    {
        var order = CreateOrder();
        order.TotalAmount.Should().Be(0m);
    }

    [Fact]
    public void AddItem_WithValidData_AddsItemToOrder()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Produto A", 2, 100m);

        order.Items.Should().HaveCount(1);
        order.Items.First().Quantity.Should().Be(2);
        order.Items.First().UnitPrice.Should().Be(100m);
    }

    [Fact]
    public void AddItem_RecalculatesTotal()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Produto A", 2, 100m);
        order.TotalAmount.Should().Be(200m);
    }

    [Fact]
    public void AddItem_MultipleItems_AccumulatesTotal()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Produto A", 2, 100m);
        order.AddItem(Guid.NewGuid(), "Produto B", 1, 50m);

        order.Items.Should().HaveCount(2);
        order.TotalAmount.Should().Be(250m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void AddItem_WithNonPositiveQuantity_ThrowsDomainException(int quantity)
    {
        var order = CreateOrder();
        var act = () => order.AddItem(Guid.NewGuid(), "Produto A", quantity, 100m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_WhenStatusIsPending_SetsStatusToCancelled()
    {
        var order = CreateOrder();
        order.Cancel();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenStatusIsProcessing_SetsStatusToCancelled()
    {
        var order = CreateOrder();
        order.UpdateStatus(OrderStatus.Processing);
        order.Cancel();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenStatusIsDelivered_ThrowsDomainException()
    {
        var order = CreateOrder();
        order.UpdateStatus(OrderStatus.Processing);
        order.UpdateStatus(OrderStatus.Shipped);
        order.UpdateStatus(OrderStatus.Delivered);

        var act = () => order.Cancel();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateStatus_WhenStatusIsCancelled_ThrowsDomainException()
    {
        var order = CreateOrder();
        order.Cancel();

        var act = () => order.UpdateStatus(OrderStatus.Processing);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateStatus_ValidTransition_ChangesStatus()
    {
        var order = CreateOrder();
        order.UpdateStatus(OrderStatus.Processing);
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void UpdateStatus_FullChain_Works()
    {
        var order = CreateOrder();
        order.UpdateStatus(OrderStatus.Processing);
        order.UpdateStatus(OrderStatus.Shipped);
        order.UpdateStatus(OrderStatus.Delivered);
        order.Status.Should().Be(OrderStatus.Delivered);
    }
}

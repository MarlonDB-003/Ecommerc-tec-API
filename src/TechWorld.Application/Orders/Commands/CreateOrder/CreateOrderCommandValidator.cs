using FluentValidation;

namespace TechWorld.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("O pedido deve conter ao menos 1 item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });

        RuleFor(x => x.CustomerInfo.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerInfo.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.CustomerInfo.Phone).NotEmpty().MaximumLength(20);

        RuleFor(x => x.AddressInfo.Cep).NotEmpty().MaximumLength(10);
        RuleFor(x => x.AddressInfo.Street).NotEmpty().MaximumLength(300);
        RuleFor(x => x.AddressInfo.Number).NotEmpty().MaximumLength(20);
        RuleFor(x => x.AddressInfo.Neighborhood).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressInfo.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressInfo.State).NotEmpty().Length(2);

        RuleFor(x => x.Installments).InclusiveBetween(1, 12);
    }
}

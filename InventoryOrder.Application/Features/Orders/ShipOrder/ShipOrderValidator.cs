using FluentValidation;

namespace InventoryOrder.Application.Features.Orders.ShipOrder;

public sealed class ShipOrderValidator : AbstractValidator<ShipOrderCommand>
{
    public ShipOrderValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.ShipmentNumber).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Lines).NotEmpty().Must(lines => lines.Count <= 100);
        RuleForEach(command => command.Lines).SetValidator(new ShipOrderLineDtoValidator());
        RuleFor(command => command.Lines)
            .Must(lines => lines.Select(line => line.SalesOrderLineId).Distinct().Count() == lines.Count)
            .WithMessage("A shipment cannot contain duplicate order lines.");
        RuleFor(command => command.Carrier).MaximumLength(100);
        RuleFor(command => command.TrackingNumber).MaximumLength(200);
        RuleFor(command => command.IdempotencyKey).NotEmpty().MaximumLength(200);
    }

    private sealed class ShipOrderLineDtoValidator : AbstractValidator<ShipOrderLineDto>
    {
        public ShipOrderLineDtoValidator()
        {
            RuleFor(line => line.SalesOrderLineId).NotEmpty();
            RuleFor(line => line.Quantity).GreaterThan(0);
        }
    }
}

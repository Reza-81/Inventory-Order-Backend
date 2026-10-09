using FluentValidation;

namespace InventoryOrder.Application.Features.Orders.PlaceOrder;

public sealed class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderValidator()
    {
        RuleFor(command => command.CustomerId)
            .NotEmpty();

        RuleFor(command => command.Lines)
            .NotEmpty()
            .Must(lines => lines.Count <= 100)
            .WithMessage("An order cannot contain more than 100 lines.");

        RuleForEach(command => command.Lines)
            .SetValidator(new OrderLineDtoValidator());

        RuleFor(command => command.Lines)
            .Must(HaveUniqueStockLines)
            .WithMessage("An order cannot contain duplicate product and warehouse lines.");

        RuleFor(command => command.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(200);
    }

    private static bool HaveUniqueStockLines(IReadOnlyCollection<OrderLineDto> lines)
    {
        return lines
            .Select(line => $"{line.Sku.Trim().ToUpperInvariant()}:{line.WarehouseId}")
            .Distinct(StringComparer.Ordinal)
            .Count() == lines.Count;
    }

    private sealed class OrderLineDtoValidator : AbstractValidator<OrderLineDto>
    {
        public OrderLineDtoValidator()
        {
            RuleFor(line => line.Sku)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(line => line.WarehouseId)
                .NotEmpty();
            RuleFor(line => line.Quantity)
                .GreaterThan(0);
        }
    }
}

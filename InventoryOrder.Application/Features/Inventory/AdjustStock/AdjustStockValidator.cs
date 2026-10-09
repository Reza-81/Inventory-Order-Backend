using FluentValidation;

namespace InventoryOrder.Application.Features.Inventory.AdjustStock;

public sealed class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockValidator()
    {
        RuleFor(command => command.WarehouseId).NotEmpty();
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Delta).NotEqual(0);
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(500);
        RuleFor(command => command.IdempotencyKey).NotEmpty().MaximumLength(200);
    }
}

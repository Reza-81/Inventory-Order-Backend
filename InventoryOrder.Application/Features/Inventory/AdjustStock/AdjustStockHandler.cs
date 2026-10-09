using InventoryOrder.Application.Common.Exceptions;
using InventoryOrder.Application.Common.Interfaces;
using InventoryOrder.Domain.Entities;
using InventoryOrder.Domain.Enums;
using MediatR;

namespace InventoryOrder.Application.Features.Inventory.AdjustStock;

public sealed class AdjustStockHandler(IApplicationDbContext dbContext)
    : IRequestHandler<AdjustStockCommand, AdjustStockResponse>
{
    public async Task<AdjustStockResponse> Handle(
        AdjustStockCommand request,
        CancellationToken cancellationToken)
    {
        _ = await dbContext.FindProductByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product was not found.");
        _ = await dbContext.FindWarehouseByIdAsync(request.WarehouseId, cancellationToken)
            ?? throw new NotFoundException("Warehouse was not found.");

        var stockItem = await dbContext.FindStockItemAsync(
            request.ProductId,
            request.WarehouseId,
            cancellationToken)
            ?? throw new NotFoundException("The product is not stocked in this warehouse.");

        stockItem.Adjust(request.Delta);
        dbContext.Add(InventoryTransaction.Create(
            request.ProductId,
            request.WarehouseId,
            stockItem.Id,
            TransactionType.Adjust,
            request.Delta,
            request.Reason,
            referenceType: nameof(AdjustStockCommand),
            referenceId: stockItem.Id));

        return new AdjustStockResponse(
            stockItem.ProductId,
            stockItem.WarehouseId,
            stockItem.QuantityOnHand,
            stockItem.QuantityReserved);
    }
}

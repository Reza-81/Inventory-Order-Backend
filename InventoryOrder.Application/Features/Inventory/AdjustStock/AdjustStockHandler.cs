using InventoryOrder.Application.Common.Exceptions;
using InventoryOrder.Application.Common.Interfaces;
using InventoryOrder.Domain.Entities;
using InventoryOrder.Domain.Enums;
using MediatR;
using InventoryOrder.Application.Common.Notifications;

namespace InventoryOrder.Application.Features.Inventory.AdjustStock;

public sealed class AdjustStockHandler(
    IApplicationDbContext dbContext,
    ICurrentUser? currentUser = null,
    IWarehouseAccessChecker? warehouseAccessChecker = null,
    IPublisher? publisher = null)
    : IRequestHandler<AdjustStockCommand, AdjustStockResponse>
{
    public async Task<AdjustStockResponse> Handle(
        AdjustStockCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser is not null && !currentUser.IsAuthenticated)
            throw new ConflictException("Authentication is required to adjust stock.");

        _ = await dbContext.FindProductByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product was not found.");
        _ = await dbContext.FindWarehouseByIdAsync(request.WarehouseId, cancellationToken)
            ?? throw new NotFoundException("Warehouse was not found.");

        if (warehouseAccessChecker is not null &&
            !await warehouseAccessChecker.CanAccessAsync(request.WarehouseId, cancellationToken))
            throw new ConflictException("You do not have access to this warehouse.");

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
        if (publisher is not null)
            await publisher.Publish(new StockChangedNotification(request.WarehouseId), cancellationToken);

        return new AdjustStockResponse(
            stockItem.ProductId,
            stockItem.WarehouseId,
            stockItem.QuantityOnHand,
            stockItem.QuantityReserved);
    }
}

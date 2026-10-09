using InventoryOrder.Application.Common.Exceptions;
using InventoryOrder.Application.Common.Interfaces;
using MediatR;

namespace InventoryOrder.Application.Features.Inventory.GetStockLevels;

public sealed class GetStockLevelsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetStockLevelsQuery, IReadOnlyCollection<StockLevelResponse>>
{
    public async Task<IReadOnlyCollection<StockLevelResponse>> Handle(
        GetStockLevelsQuery request,
        CancellationToken cancellationToken)
    {
        _ = await dbContext.FindWarehouseByIdAsync(request.WarehouseId, cancellationToken)
            ?? throw new NotFoundException("Warehouse was not found.");

        var stockLevels = await dbContext.GetStockLevelsByWarehouseAsync(
            request.WarehouseId,
            cancellationToken);

        return stockLevels
            .Select(stock => new StockLevelResponse(
                stock.ProductId,
                stock.Sku,
                stock.QuantityOnHand,
                stock.QuantityReserved,
                stock.QuantityOnHand - stock.QuantityReserved))
            .ToArray();
    }
}

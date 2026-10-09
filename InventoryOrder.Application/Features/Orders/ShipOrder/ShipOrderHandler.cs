using InventoryOrder.Application.Common.Exceptions;
using InventoryOrder.Application.Common.Interfaces;
using InventoryOrder.Domain.Entities;
using InventoryOrder.Domain.Enums;
using MediatR;
using InventoryOrder.Application.Common.Notifications;

namespace InventoryOrder.Application.Features.Orders.ShipOrder;

public sealed class ShipOrderHandler(IApplicationDbContext dbContext, IPublisher? publisher = null)
    : IRequestHandler<ShipOrderCommand, ShipOrderResponse>
{
    public async Task<ShipOrderResponse> Handle(
        ShipOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await dbContext.FindSalesOrderWithLinesAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Sales order was not found.");

        if (order.Status is OrderStatus.Draft or OrderStatus.Cancelled or OrderStatus.Shipped)
            throw new ConflictException("Only a confirmed or partially shipped order can be shipped.");

        var shipment = Shipment.Create(
            request.ShipmentNumber,
            order.Id,
            request.Carrier,
            request.TrackingNumber);

        foreach (var requestedLine in request.Lines)
        {
            var orderLine = order.Lines.SingleOrDefault(line => line.Id == requestedLine.SalesOrderLineId)
                ?? throw new NotFoundException(
                    $"Sales order line '{requestedLine.SalesOrderLineId}' was not found on this order.");

            var remainingQuantity = orderLine.QuantityOrdered - orderLine.QuantityShipped;
            if (requestedLine.Quantity > remainingQuantity)
                throw new ConflictException("Shipment quantity exceeds the unshipped order quantity.");

            var stockItem = await dbContext.FindStockItemAsync(
                orderLine.ProductId,
                orderLine.WarehouseId,
                cancellationToken)
                ?? throw new NotFoundException("The order line's stock item was not found.");

            stockItem.Ship(requestedLine.Quantity);
            orderLine.Ship(requestedLine.Quantity);

            shipment.AddLine(ShipmentLine.Create(
                shipment.Id,
                orderLine.Id,
                requestedLine.Quantity));

            dbContext.Add(InventoryTransaction.Create(
                orderLine.ProductId,
                orderLine.WarehouseId,
                stockItem.Id,
                TransactionType.Ship,
                requestedLine.Quantity,
                reason: "Sales order shipment",
                referenceType: nameof(Shipment),
                referenceId: shipment.Id));
        }

        shipment.MarkShipped();
        order.UpdateShipmentStatus();
        dbContext.Add(shipment);

        if (publisher is not null)
        {
            foreach (var warehouseId in order.Lines
                         .Where(line => request.Lines.Any(requested => requested.SalesOrderLineId == line.Id))
                         .Select(line => line.WarehouseId)
                         .Distinct())
                await publisher.Publish(new StockChangedNotification(warehouseId), cancellationToken);
        }

        return new ShipOrderResponse(
            shipment.Id,
            shipment.ShipmentNumber,
            order.Id,
            order.Status.ToString(),
            shipment.Status.ToString());
    }
}

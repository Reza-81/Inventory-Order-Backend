using InventoryOrder.Application.Common.Exceptions;
using InventoryOrder.Application.Common.Interfaces;
using InventoryOrder.Domain.Entities;
using MediatR;

namespace InventoryOrder.Application.Features.Orders.PlaceOrder;

public sealed class PlaceOrderHandler(IApplicationDbContext dbContext)
    : IRequestHandler<PlaceOrderCommand, PlaceOrderResponse>
{
    public async Task<PlaceOrderResponse> Handle(
        PlaceOrderCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await dbContext.FindCustomerByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundException("Customer was not found.");

        if (!customer.IsActive)
            throw new ConflictException("An order cannot be placed for an inactive customer.");

        var resolvedLines = new List<ResolvedOrderLine>(request.Lines.Count);
        foreach (var requestedLine in request.Lines)
        {
            var product = await dbContext.FindProductBySkuAsync(
                requestedLine.Sku.Trim().ToUpperInvariant(),
                cancellationToken)
                ?? throw new NotFoundException($"Product '{requestedLine.Sku}' was not found.");

            if (!product.IsActive)
                throw new ConflictException($"Product '{requestedLine.Sku}' is inactive.");

            var stockItem = await dbContext.FindStockItemAsync(
                product.Id,
                requestedLine.WarehouseId,
                cancellationToken)
                ?? throw new NotFoundException(
                    $"No stock item exists for product '{requestedLine.Sku}' in warehouse '{requestedLine.WarehouseId}'.");

            stockItem.Reserve(requestedLine.Quantity);
            resolvedLines.Add(new ResolvedOrderLine(product, stockItem, requestedLine.Quantity));
        }

        var currency = resolvedLines[0].Product.Price.Currency;
        if (resolvedLines.Any(line => !string.Equals(line.Product.Price.Currency, currency, StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException("All products in an order must use the same currency.");

        var order = SalesOrder.Create(
            GenerateOrderNumber(),
            customer.Id,
            currency);

        foreach (var resolvedLine in resolvedLines)
        {
            var orderLine = SalesOrderLine.Create(
                order.Id,
                resolvedLine.Product.Id,
                resolvedLine.StockItem.WarehouseId,
                resolvedLine.Quantity,
                resolvedLine.Product.Price.Amount,
                currency);

            order.AddLine(orderLine);
            dbContext.Add(InventoryTransaction.Create(
                resolvedLine.Product.Id,
                resolvedLine.StockItem.WarehouseId,
                resolvedLine.StockItem.Id,
                Domain.Enums.TransactionType.Reserve,
                resolvedLine.Quantity,
                reason: "Sales order reservation",
                referenceType: nameof(SalesOrder),
                referenceId: order.Id));
        }

        order.Confirm();
        dbContext.Add(order);

        return new PlaceOrderResponse(
            order.Id,
            order.OrderNumber,
            order.TotalAmount,
            order.Currency,
            order.Status.ToString());
    }

    private static string GenerateOrderNumber() =>
        $"ORD-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..30].ToUpperInvariant();

    private sealed record ResolvedOrderLine(Product Product, StockItem StockItem, decimal Quantity);
}

using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace Map;

public class ValidateOrderBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order extractedValue)
    {
        if (extractedValue.Amount <= 0)
            parameter.SignalBreak("Amount less than or equal to 0");
        return extractedValue;
    }
}

public class PlaceOrderBlock : AsyncCodeBlock<Order, Payment>
{
    protected override async ValueTask<Parameter<Payment>> ExecuteAsync(Parameter<Order> parameter, Order extractedValue)
    {
        await Task.Delay(100); // Simulate payment gateway

        return new Payment
        {
            OrderId = extractedValue.OrderId,
            TransactionId = Random.Shared.Next(10000, 100000).ToString(),
            Amount = extractedValue.Amount,
            ProcessedAt = DateTime.UtcNow
        };
    }
}

public class ReceiptBlock : CodeBlock<Payment, Receipt>
{
    protected override Parameter<Receipt> Execute(Parameter<Payment> parameter, Payment extractedValue)
        => new Receipt
        {
            OrderId = extractedValue.OrderId,
            ReceiptNumber = $"RCP-{extractedValue.TransactionId}",
            Total = extractedValue.Amount
        };
}

public class ShipmentBlock : CodeBlock<Receipt, Shipment>
{
    protected override Parameter<Shipment> Execute(Parameter<Receipt> parameter, Receipt extractedValue)
        => new Shipment
        {
            OrderId = extractedValue.OrderId,
            TrackingNumber = $"TR-{extractedValue.ReceiptNumber}",
            EstimatedDelivery = DateTime.UtcNow.AddDays(Random.Shared.Next(1, 15))
        };
}
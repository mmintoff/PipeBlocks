using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace Map;

public class ValidateOrderBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order extractedValue)
        => parameter;
}

public class PlaceOrderBlock : CodeBlock<Order, Payment>
{
    protected override Parameter<Payment> Execute(Parameter<Order> parameter, Order extractedValue)
        => new Payment { OrderId = extractedValue.OrderId };
}

public class ReceiptBlock : CodeBlock<Payment, Receipt>
{
    protected override Parameter<Receipt> Execute(Parameter<Payment> parameter, Payment extractedValue)
        => new Receipt { OrderId = extractedValue.OrderId };
}

public class ShipmentBlock : CodeBlock<Receipt, Shipment>
{
    protected override Parameter<Shipment> Execute(Parameter<Receipt> parameter, Receipt extractedValue)
        => new Shipment { OrderId = extractedValue.OrderId };
}
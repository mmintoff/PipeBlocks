namespace Map;

public class Order
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public required string CustomerEmail { get; set; }
}

public class Payment
{
    public int OrderId { get; set; }
    public required string TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class Receipt
{
    public int OrderId { get; set; }
    public required string ReceiptNumber { get; set; }
    public decimal Total { get; set; }
}

public class Shipment
{
    public int OrderId { get; set; }
    public required string TrackingNumber { get; set; }
    public DateTime EstimatedDelivery { get; set; }
}
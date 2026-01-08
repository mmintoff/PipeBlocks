namespace Map;

public class Order
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CustomerEmail { get; set; }
}

public class Payment
{
    public int OrderId { get; set; }
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class Receipt
{
    public int OrderId { get; set; }
    public string ReceiptNumber { get; set; }
    public decimal Total { get; set; }
}

public class Shipment
{
    public int OrderId { get; set; }
    public string TrackingNumber { get; set; }
    public DateTime EstimatedDelivery { get; set; }
}
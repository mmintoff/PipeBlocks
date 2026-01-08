namespace Map;

public class Order
{
    public required long OrderId { get; set; }
}
public class Payment
{
    public required long OrderId { get; set; }
}
public class Receipt
{
    public required long OrderId { get; set; }
}
public class Shipment
{
    public required long OrderId { get; set; }
}
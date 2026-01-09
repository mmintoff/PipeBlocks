using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Test;

public class MapPipeBlockTests
{
    private readonly BlockBuilder<OrderData> _builder;

    public MapPipeBlockTests()
    {
        var loggerFactory = NullLoggerFactory.Instance;
        _builder = new BlockBuilder<OrderData>(loggerFactory);
    }

    [Fact]
    public void Execute_HappyPath_TransformsThroughAllTypes()
    {
        // Arrange
        var pipe = _builder
            .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "test-pipe" }))
            .Then(new ValidateOrderBlock())
            .Map<PaymentData>().Via(new OrderToPaymentBlock())
            .Map<ReceiptData>().Via(new PaymentToReceiptBlock())
            .Map<ShipmentData>().Via(new ReceiptToShipmentBlock());

        var order = new OrderData
        {
            OrderId = 12345,
            Amount = 99.99m,
            CustomerEmail = "test@example.com"
        };

        // Act
        var result = pipe.Execute(new Parameter<OrderData>(order));

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(result.TryGetValue(out var shipment));
        Assert.NotNull(shipment);
        Assert.Equal(12345, shipment.OrderId);
        Assert.NotNull(shipment.TrackingNumber);
        Assert.NotEmpty(shipment.TrackingNumber);
        Assert.True(shipment.EstimatedDelivery > DateTime.UtcNow);
    }

    [Fact]
    public async Task ExecuteAsync_HappyPath_TransformsThroughAllTypes()
    {
        // Arrange
        var pipe = _builder
            .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "test-pipe-async" }))
            .Then(new ValidateOrderBlock())
            .Map<PaymentData>().Via(new OrderToPaymentBlock())
            .Map<ReceiptData>().Via(new PaymentToReceiptBlock())
            .Map<ShipmentData>().Via(new ReceiptToShipmentBlock());

        var order = new OrderData
        {
            OrderId = 67890,
            Amount = 149.99m,
            CustomerEmail = "async@example.com"
        };

        // Act
        var result = await pipe.ExecuteAsync(new Parameter<OrderData>(order));

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(result.TryGetValue(out var shipment));
        Assert.NotNull(shipment);
        Assert.Equal(67890, shipment.OrderId);
        Assert.NotNull(shipment.TrackingNumber);
    }

    [Fact]
    public void Execute_ContextPersisted_ThroughAllTransformations()
    {
        // Arrange
        var pipe = _builder
            .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "context-test-pipe" }))
            .Then(new EnrichOrderBlock()) // Sets context data
            .Map<PaymentData>().Via(new OrderToPaymentBlock())
            .Map<ReceiptData>().Via(new PaymentToReceiptBlock())
            .Map<ShipmentData>().Via(new ReceiptToShipmentBlock());

        var order = new OrderData { OrderId = 111, Amount = 50m, CustomerEmail = "test@example.com" };
        var parameter = new Parameter<OrderData>(order);
        var originalCorrelationId = parameter.CorrelationId;

        // Act
        var result = pipe.Execute(parameter);

        // Assert
        Assert.False(result.IsFailure);

        // Correlation ID preserved
        Assert.Equal(originalCorrelationId, result.CorrelationId);

        // Context data set in EnrichOrderBlock should still be present
        Assert.True(result.Context.TryGet<string>("EnrichedBy", out var enrichedBy));
        Assert.Equal("EnrichOrderBlock", enrichedBy);

        Assert.True(result.Context.TryGet<DateTime>("EnrichedAt", out var enrichedAt));
        var timeDiff = DateTime.UtcNow - enrichedAt;
        Assert.True(timeDiff.TotalSeconds < 5, $"EnrichedAt timestamp should be recent, but was {timeDiff.TotalSeconds} seconds ago");
    }

    [Fact]
    public async Task ExecuteAsync_ContextPersisted_ThroughAllTransformations()
    {
        // Arrange
        var pipe = _builder
            .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "context-test-pipe-async" }))
            .Then(new EnrichOrderBlock())
            .Map<PaymentData>().Via(new OrderToPaymentBlock())
            .Map<ReceiptData>().Via(new PaymentToReceiptBlock())
            .Map<ShipmentData>().Via(new ReceiptToShipmentBlock());

        var order = new OrderData { OrderId = 222, Amount = 75m, CustomerEmail = "test@example.com" };
        var parameter = new Parameter<OrderData>(order);
        var originalCorrelationId = parameter.CorrelationId;

        // Act
        var result = await pipe.ExecuteAsync(parameter);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(originalCorrelationId, result.CorrelationId);
        Assert.True(result.Context.TryGet<string>("EnrichedBy", out _));
    }

    [Fact]
    public void Execute_FailureAtMappingStep_PreventsFurtherExecution()
    {
        // Arrange
        var pipe = _builder
            .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "failing-pipe" }))
            .Then(new ValidateOrderBlock())
            .Map<PaymentData>().Via(new FailingPaymentBlock()) // Fails here
            .Map<ReceiptData>().Via(new PaymentToReceiptBlock()) // Should not execute
            .Map<ShipmentData>().Via(new ReceiptToShipmentBlock()); // Should not execute

        var order = new OrderData
        {
            OrderId = 333,
            Amount = 10m, // Low amount triggers failure
            CustomerEmail = "fail@example.com"
        };

        // Act
        var result = pipe.Execute(new Parameter<OrderData>(order));

        // Assert
        Assert.True(result.IsFailure);

        result.Match(
            failure =>
            {
                Assert.Contains("Payment failed", failure.FailureReason);
                // The failure should contain the OrderData input
                Assert.True(failure.TryGetValue<OrderData>(out var failedOrder));
                Assert.NotNull(failedOrder);
                Assert.Equal(333, failedOrder.OrderId);
            },
            success => Assert.Fail("Should have failed but succeeded")
        );
    }

    [Fact]
    public async Task ExecuteAsync_FailureAtMappingStep_PreventsFurtherExecution()
    {
        // Arrange
        var pipe = _builder
            .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "failing-pipe-async" }))
            .Then(new ValidateOrderBlock())
            .Map<PaymentData>().Via(new FailingPaymentBlock())
            .Map<ReceiptData>().Via(new PaymentToReceiptBlock())
            .Map<ShipmentData>().Via(new ReceiptToShipmentBlock());

        var order = new OrderData { OrderId = 444, Amount = 5m, CustomerEmail = "fail@example.com" };

        // Act
        var result = await pipe.ExecuteAsync(new Parameter<OrderData>(order));

        // Assert
        Assert.True(result.IsFailure);
        result.Match(
            failure => Assert.Contains("Payment failed", failure.FailureReason),
            success => Assert.Fail("Should have failed but succeeded")
        );
    }

    [Fact]
    public void Execute_MixHomogeneousAndHeterogeneous_WorksCorrectly()
    {
        // Arrange
        var pipe = _builder
            .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "mixed-pipe" }))
            .Then(new ValidateOrderBlock()) // Order → Order
            .Then(new EnrichOrderBlock()) // Order → Order
            .Map<PaymentData>().Via(new OrderToPaymentBlock()) // Order → Payment
            .Map<ReceiptData>().Via(new PaymentToReceiptBlock()) // Payment → Receipt
            .Map<ShipmentData>().Via(new ReceiptToShipmentBlock()); // Receipt → Shipment

        var order = new OrderData { OrderId = 555, Amount = 125m, CustomerEmail = "test@example.com" };

        // Act
        var result = pipe.Execute(new Parameter<OrderData>(order));

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(result.TryGetValue(out var shipment));
        Assert.NotNull(shipment);
        Assert.Equal(555, shipment.OrderId);
    }

    [Fact]
    public void Execute_ChainAfterMapping_WorksCorrectly()
    {
        // Arrange
        var pipe = _builder
            .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "chain-after-map-pipe" }))
            .Then(new ValidateOrderBlock())
            .Map<PaymentData>().Via(new OrderToPaymentBlock())
            .Then(new LogPaymentBlock()) // Chained homogeneous block after mapping
            .Map<ReceiptData>().Via(new PaymentToReceiptBlock());

        var order = new OrderData { OrderId = 666, Amount = 200m, CustomerEmail = "test@example.com" };

        // Act
        var result = pipe.Execute(new Parameter<OrderData>(order));

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(result.TryGetValue(out var receipt));
        Assert.NotNull(receipt);
        Assert.Equal(666, receipt.OrderId);

        // Verify LogPaymentBlock set context data
        Assert.True(result.Context.TryGet<bool>("PaymentLogged", out var logged));
        Assert.True(logged);
    }

    [Fact]
    public void Execute_ContextMerge_PreservesEarlyAndLateData()
    {
        // Arrange
        var pipe = _builder
            .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "context-merge-pipe" }))
            .Then(new EnrichOrderBlock()) // Sets "EnrichedBy" and "EnrichedAt"
            .Map<PaymentData>().Via(new OrderToPaymentBlock()) // Adds "TransactionId"
            .Map<ReceiptData>().Via(new PaymentToReceiptBlock()) // Adds "ReceiptNumber"
            .Map<ShipmentData>().Via(new ReceiptToShipmentBlock()); // Adds "TrackingNumber"

        var order = new OrderData { OrderId = 777, Amount = 99m, CustomerEmail = "test@example.com" };
        var parameter = new Parameter<OrderData>(order);

        // Act
        var result = pipe.Execute(parameter);

        // Assert
        Assert.False(result.IsFailure);

        // Data from early block (EnrichOrderBlock)
        Assert.True(result.Context.TryGet<string>("EnrichedBy", out var enrichedBy));
        Assert.Equal("EnrichOrderBlock", enrichedBy);

        // Data from mapping blocks
        Assert.True(result.Context.TryGet<string>("TransactionId", out var transactionId));
        Assert.NotNull(transactionId);

        Assert.True(result.Context.TryGet<string>("ReceiptNumber", out var receiptNumber));
        Assert.NotNull(receiptNumber);

        Assert.True(result.Context.TryGet<string>("TrackingNumber", out var trackingNumber));
        Assert.NotNull(trackingNumber);
    }

    [Fact]
    public void Execute_FailurePreservesInputType_ForDebugging()
    {
        // Arrange
        var pipe = _builder
            .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "failure-input-pipe" }))
            .Then(new ValidateOrderBlock())
            .Map<PaymentData>().Via(new FailingPaymentBlock());

        var order = new OrderData
        {
            OrderId = 888,
            Amount = 1m, // Triggers failure
            CustomerEmail = "fail@example.com"
        };

        // Act
        var result = pipe.Execute(new Parameter<OrderData>(order));

        // Assert
        Assert.True(result.IsFailure);

        result.Match(
            failure =>
            {
                // Should preserve the Order (input) that caused the failure
                Assert.True(failure.TryGetValue<OrderData>(out var failedOrder));
                Assert.NotNull(failedOrder);
                Assert.Equal(888, failedOrder.OrderId);
                Assert.Equal(1m, failedOrder.Amount);
                Assert.Equal("fail@example.com", failedOrder.CustomerEmail);
            },
            success => Assert.Fail("Should have failed")
        );
    }

    [Fact]
    public void Execute_MultipleContextModifications_AllPreserved()
    {
        // Arrange
        var pipe = _builder
            .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "multi-context-pipe" }))
            .Then(new EnrichOrderBlock())
            .Map<PaymentData>().Via(new OrderToPaymentBlock())
            .Then(new LogPaymentBlock())
            .Map<ReceiptData>().Via(new PaymentToReceiptBlock());

        var order = new OrderData { OrderId = 999, Amount = 150m, CustomerEmail = "test@example.com" };
        var parameter = new Parameter<OrderData>(order);
        parameter.Context.Set("CustomField", "CustomValue");

        // Act
        var result = pipe.Execute(parameter);

        // Assert
        Assert.False(result.IsFailure);

        // Original custom field preserved
        Assert.True(result.Context.TryGet<string>("CustomField", out var customValue));
        Assert.Equal("CustomValue", customValue);

        // EnrichOrderBlock data preserved
        Assert.True(result.Context.TryGet<string>("EnrichedBy", out _));

        // OrderToPaymentBlock data preserved
        Assert.True(result.Context.TryGet<string>("TransactionId", out _));

        // LogPaymentBlock data preserved
        Assert.True(result.Context.TryGet<bool>("PaymentLogged", out var logged));
        Assert.True(logged);

        // PaymentToReceiptBlock data preserved
        Assert.True(result.Context.TryGet<string>("ReceiptNumber", out _));
    }
}

// Test Data Models
public class OrderData
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
}

public class PaymentData
{
    public int OrderId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class ReceiptData
{
    public int OrderId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class ShipmentData
{
    public int OrderId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime EstimatedDelivery { get; set; }
}

// Test Blocks
public class ValidateOrderBlock : CodeBlock<OrderData>
{
    protected override Parameter<OrderData> Execute(Parameter<OrderData> parameter, OrderData order)
    {
        if (order.Amount <= 0)
        {
            parameter.SignalBreak("Amount must be positive");
        }
        return parameter;
    }
}

public class EnrichOrderBlock : CodeBlock<OrderData>
{
    protected override Parameter<OrderData> Execute(Parameter<OrderData> parameter, OrderData order)
    {
        parameter.Context.Set("EnrichedBy", "EnrichOrderBlock");
        parameter.Context.Set("EnrichedAt", DateTime.UtcNow);
        return parameter;
    }
}

public class OrderToPaymentBlock : CodeBlock<OrderData, PaymentData>
{
    protected override Parameter<PaymentData> Execute(Parameter<OrderData> parameter, OrderData order)
    {
        var payment = new PaymentData
        {
            OrderId = order.OrderId,
            TransactionId = $"TXN-{Guid.NewGuid():N}",
            Amount = order.Amount,
            ProcessedAt = DateTime.UtcNow
        };

        parameter.Context.Set("TransactionId", payment.TransactionId);

        return new Parameter<PaymentData>(payment);
    }
}

public class PaymentToReceiptBlock : CodeBlock<PaymentData, ReceiptData>
{
    protected override Parameter<ReceiptData> Execute(Parameter<PaymentData> parameter, PaymentData payment)
    {
        var receipt = new ReceiptData
        {
            OrderId = payment.OrderId,
            ReceiptNumber = $"RCP-{payment.TransactionId}",
            Total = payment.Amount
        };

        parameter.Context.Set("ReceiptNumber", receipt.ReceiptNumber);

        return new Parameter<ReceiptData>(receipt);
    }
}

public class ReceiptToShipmentBlock : CodeBlock<ReceiptData, ShipmentData>
{
    protected override Parameter<ShipmentData> Execute(Parameter<ReceiptData> parameter, ReceiptData receipt)
    {
        var shipment = new ShipmentData
        {
            OrderId = receipt.OrderId,
            TrackingNumber = $"SHIP-{Guid.NewGuid():N}",
            EstimatedDelivery = DateTime.UtcNow.AddDays(3)
        };

        parameter.Context.Set("TrackingNumber", shipment.TrackingNumber);

        return new Parameter<ShipmentData>(shipment);
    }
}

public class FailingPaymentBlock : CodeBlock<OrderData, PaymentData>
{
    protected override Parameter<PaymentData> Execute(Parameter<OrderData> parameter, OrderData order)
    {
        if (order.Amount < 20m)
        {
            parameter.SignalBreak("Payment failed: Amount too low");
            return new Parameter<PaymentData>(new DefaultFailureState<OrderData>(order)
            {
                FailureReason = "Payment failed: Amount too low"
            });
        }

        var payment = new PaymentData
        {
            OrderId = order.OrderId,
            TransactionId = $"TXN-{Guid.NewGuid():N}",
            Amount = order.Amount,
            ProcessedAt = DateTime.UtcNow
        };

        return new Parameter<PaymentData>(payment);
    }
}

public class LogPaymentBlock : CodeBlock<PaymentData>
{
    protected override Parameter<PaymentData> Execute(Parameter<PaymentData> parameter, PaymentData payment)
    {
        parameter.Context.Set("PaymentLogged", true);
        parameter.Context.Set("LoggedAt", DateTime.UtcNow);
        return parameter;
    }
}
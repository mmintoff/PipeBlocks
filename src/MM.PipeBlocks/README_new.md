# MM.PipeBlocks

## Introduction

**MM.PipeBlocks** is a composable, modular pipeline library for .NET that enables process-oriented programming with sequential execution, branching, error handling, and integrated async/sync support. Encapsulate business logic into reusable "blocks" that chain together to form complex workflows while maintaining clarity, testability, and early failure semantics.

Whether you're processing data through multiple stages, orchestrating microservices, or implementing complex business workflows, MM.PipeBlocks provides a structured, functional approach to handling process flows that eliminates the complexity of deeply nested control structures and scattered error handling.

## Purpose

Pipeline-oriented programming is an effort to simplify process flows in programming by establishing a mono-directional flow—logic moves in one direction only. This package supports mono-directional flow with branching capabilities, making complex workflows easy to understand, debug, and maintain.

Instead of traditional nested conditional logic where services call other services (creating a tangled web of dependencies and control flow), MM.PipeBlocks enforces a clean, linear progression through discrete, composable steps.

## Why MM.PipeBlocks?

### The Problem: Traditional C# Approaches

Modern C# applications often suffer from several architectural challenges:

#### 1. **Deeply Nested Control Flow**
Traditional approaches often lead to nested `if` statements and branching logic that becomes increasingly difficult to follow:

```csharp
// ? Traditional Approach - Hard to follow
public async Task<Result> ProcessOrder(Order order)
{
    if (order != null)
    {
        if (await ValidateOrder(order))
        {
            if (await CheckInventory(order))
            {
                if (await ProcessPayment(order))
                {
                    if (await CreateShipment(order))
                    {
                        if (await SendNotification(order))
                        {
                            return Result.Success;
                        }
                    }
                }
            }
        }
    }
    return Result.Failure;
}
```

#### 2. **Service-Calling-Service Complexity**
Tracking data flow becomes nearly impossible when services call other services, which call other services. Context gets lost, and it's hard to understand what transformations occur at each step:

```csharp
// ? Service Soup - Hard to track data flow
public async Task<OrderResult> HandleOrderAsync(Order order)
{
    var validated = await _validationService.ValidateAsync(order);
    if (!validated.IsValid)
        return OrderResult.Failure(validated.Errors);
    
    var inventory = await _inventoryService.ReserveAsync(order);
    if (!inventory.IsAvailable)
        return OrderResult.Failure("Out of stock");
    
    var payment = await _paymentService.ChargeAsync(order.Total);
    if (!payment.IsSuccess)
    {
        await _inventoryService.ReleaseAsync(order);
        return OrderResult.Failure("Payment failed");
    }
    
    var shipment = await _shippingService.CreateShipmentAsync(order);
    if (shipment == null)
    {
        await _paymentService.RefundAsync(payment.TransactionId);
        await _inventoryService.ReleaseAsync(order);
        return OrderResult.Failure("Shipment creation failed");
    }
    
    return OrderResult.Success(shipment);
}
```

#### 3. **Scattered Error Handling**
Error handling logic is spread throughout the codebase, making it difficult to maintain consistent error handling strategies:

```csharp
// ? Scattered Error Handling
try
{
    var step1 = DoStep1();
}
catch (ValidationException ex)
{
    logger.LogError(ex, "Step 1 failed");
    return CreateErrorResponse(ex);
}

try
{
    var step2 = DoStep2(step1);
}
catch (TimeoutException ex)
{
    logger.LogError(ex, "Step 2 timed out");
    // Different error handling logic
    return HandleTimeout(ex);
}

// ... pattern repeats for each step
```

#### 4. **Lack of Composability**
Individual operations are tightly coupled and difficult to reuse, test independently, or compose into different workflows:

```csharp
// ? Tightly Coupled Components
public class OrderProcessor
{
    public async Task<Result> ProcessAsync(Order order)
    {
        // Validation logic embedded here
        if (string.IsNullOrEmpty(order.CustomerEmail))
            return Result.Failure("Email required");
        
        // Payment logic embedded here
        var result = await _paymentGateway.ChargeAsync(order.Total);
        
        // Shipping logic embedded here
        await _shippingProvider.CreateAsync(order);
        
        // All tightly coupled - hard to reuse parts independently
    }
}
```

#### 5. **Mixing Async/Sync Complexity**
Managing mixed async and sync operations becomes a headache, often leading to deadlocks or performance issues:

```csharp
// ? Async/Sync Mixing Complexity
public async Task<Result> ProcessAsync(Order order)
{
    // Sync operation mixed with async
    ValidateSync(order); // Blocks the async flow
    
    await FetchDataAsync(order);
    
    // Another sync operation
    var calculated = CalculateAsync(order).Result; // ?? DEADLOCK RISK!
    
    await SendNotificationAsync(order);
}
```

#### 6. **Lost Context Across Steps**
As data flows through multiple service calls, contextual information (logging correlation IDs, user context, execution state) is often lost or forgotten:

```csharp
// ? Lost Context
public async Task<Order> ProcessAsync(Order order)
{
    // Correlation ID lost after first service call
    var step1 = await service1.DoWorkAsync(order); // Who tracks correlation?
    var step2 = await service2.DoWorkAsync(step1);  // No context carried
    var step3 = await service3.DoWorkAsync(step2);  // Lost traceability
}
```

#### 7. **Defensive Programming Instead of Happy Path**
Without a structured approach, you must constantly validate inputs and handle edge cases at every step, leading to bloated code:

```csharp
// ? Defensive Everywhere
public async Task<Result> ProcessAsync(Order order)
{
    if (order == null)
        return Result.Failure("Order is null");
    
    if (order.Items == null || order.Items.Count == 0)
        return Result.Failure("No items");
    
    if (string.IsNullOrEmpty(order.CustomerEmail))
        return Result.Failure("Email required");
    
    var result = await DoWork(order);
    if (result == null)
        return Result.Failure("Work returned null");
    
    // ... and so on, defensive checks everywhere
}
```

---

### The Solution: MM.PipeBlocks

MM.PipeBlocks solves all these issues with a **two-rail system** based on functional programming principles:

? **Clean, Linear Flow** - Execute steps in a single direction  
? **Early Failure** - Stop processing on failure and propagate errors  
? **Unified Error Handling** - One consistent error handling strategy  
? **Fully Composable** - Reuse blocks in different pipelines  
? **Mixed Async/Sync** - Handle both seamlessly  
? **Preserved Context** - Carry data and context through the entire pipeline  
? **Happy Path Coding** - Write for the success case; failures are handled automatically  

---

## Core Concepts

### The Two-Rail System

MM.PipeBlocks implements a two-rail (Either) monad pattern where each step's result is either a **success** (right rail) or a **failure** (left rail). Once a failure occurs, processing stops immediately and the failure state is carried through to the end.

```csharp
// Each step either succeeds or fails
Parameter<OrderData>
    ?? Success Path ??? Validate ??? Process ??? Confirm ??? Result
    ?? Failure Path ??? [STOP] ???????????????? Return Error
```

### Blocks

A **Block** is the fundamental unit of work in MM.PipeBlocks. Each block:
- Receives a `Parameter<T>` containing your data and context
- Performs a single, focused operation
- Returns the updated `Parameter<T>`
- If a failure occurs, signals it and stops the pipeline

```csharp
public class ValidateOrderBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        if (order.Items.Count == 0)
        {
            parameter.SignalBreak(new DefaultFailureState<Order>(order)
            {
                FailureReason = "Order must contain at least one item"
            });
        }
        return parameter;
    }
}
```

### Parameters and Context

The `Parameter<T>` class wraps your data and provides a `Context` object that persists throughout the pipeline execution:

```csharp
public class Parameter<V>
{
    public V Value { get; }                    // Your actual data
    public Context Context { get; set; }       // Shared context
    public Guid CorrelationId { get; }         // For tracing
}

public class Context
{
    public Guid CorrelationId { get; set; }   // Trace ID
    public bool IsFinished { get; set; }      // Pipeline complete?
    public Dictionary<string, object> Items { get; set; } // Shared state
}
```

### Pipelines

A **Pipeline** chains multiple blocks together. Blocks execute sequentially, with each block receiving the result of the previous one:

```csharp
var pipe = builder.CreatePipe(options)
    .Then<ValidateOrderBlock>()
    .Then<CheckInventoryBlock>()
    .Then<ProcessPaymentBlock>()
    .Then<CreateShipmentBlock>()
    .Then<SendNotificationBlock>();

// Execute the pipeline
var result = pipe.Execute(new Parameter<Order>(order));

result.Match(
    failure => Console.WriteLine($"Failed: {failure.FailureReason}"),
    success => Console.WriteLine($"Success: Order {success.OrderId}")
);
```

---

## Quick Start

### 1. Define Your Data Model

```csharp
public class OrderData
{
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; }
}
```

### 2. Create Blocks

**Synchronous Block:**
```csharp
public class ValidateOrderBlock : CodeBlock<OrderData>
{
    protected override Parameter<OrderData> Execute(Parameter<OrderData> parameter, OrderData order)
    {
        // Code for success case only
        if (order.Amount <= 0)
        {
            parameter.SignalBreak(new DefaultFailureState<OrderData>(order)
            {
                FailureReason = "Amount must be positive"
            });
        }
        
        order.Status = "Validated";
        return parameter;
    }
}
```

**Asynchronous Block:**
```csharp
public class ProcessPaymentBlock : AsyncCodeBlock<OrderData>
{
    private readonly IPaymentGateway _gateway;
    
    public ProcessPaymentBlock(IPaymentGateway gateway) => _gateway = gateway;
    
    protected override async ValueTask<Parameter<OrderData>> ExecuteAsync(
        Parameter<OrderData> parameter, 
        OrderData order)
    {
        try
        {
            var paymentResult = await _gateway.ChargeAsync(order.Amount);
            if (!paymentResult.IsSuccess)
            {
                parameter.SignalBreak(new DefaultFailureState<OrderData>(order)
                {
                    FailureReason = $"Payment failed: {paymentResult.ErrorMessage}"
                });
            }
            else
            {
                order.Status = "Paid";
            }
        }
        catch (Exception ex)
        {
            parameter.SignalBreak(new DefaultFailureState<OrderData>(order)
            {
                FailureReason = $"Payment error: {ex.Message}"
            });
        }
        
        return parameter;
    }
}
```

### 3. Build and Execute the Pipeline

```csharp
// Setup
var serviceCollection = new ServiceCollection();
serviceCollection.AddTransient<IPaymentGateway, PaymentGateway>();
serviceCollection.AddTransient<ValidateOrderBlock>();
serviceCollection.AddTransient<ProcessPaymentBlock>();
serviceCollection.AddLogging();

var serviceProvider = serviceCollection.BuildServiceProvider();
var builder = new BlockBuilder<OrderData>(serviceProvider.GetRequiredService<ILoggerFactory>());

// Create pipeline
var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions 
{ 
    PipeName = "Order Processing" 
}))
    .Then<ValidateOrderBlock>()
    .Then<ProcessPaymentBlock>();

// Execute
var order = new OrderData
{
    OrderId = "ORD-001",
    Amount = 99.99m,
    CustomerEmail = "customer@example.com",
    CreatedAt = DateTime.UtcNow,
    Status = "Created"
};

var result = pipe.Execute(new Parameter<OrderData>(order));

// Handle result using two-rail pattern
result.Match(
    failure =>
    {
        Console.WriteLine($"? Failed: {failure.FailureReason}");
        Console.WriteLine($"   Order: {failure.Value.OrderId}");
    },
    success =>
    {
        Console.WriteLine($"? Success: Order {success.OrderId} - {success.Status}");
    }
);
```

---

## Advanced Features

### Branching Logic

Execute different blocks based on conditions:

```csharp
var pipe = builder.CreatePipe(options)
    .Then<ValidateOrderBlock>()
    .Then(b => b.Switch(parameter => parameter.Value.Amount > 1000 
        ? b.ResolveInstance<PremiumProcessingBlock>()
        : b.ResolveInstance<StandardProcessingBlock>()
    ))
    .Then<SendConfirmationBlock>();
```

### Try/Catch Blocks

Handle errors gracefully with recovery logic:

```csharp
var pipe = builder.CreatePipe(options)
    .Then(b => b.TryCatch<ProcessPaymentBlock, PaymentErrorRecoveryBlock>())
    .Then<SendConfirmationBlock>();
```

### Parallel Processing

Execute multiple blocks in parallel:

```csharp
var pipe = builder.CreatePipe(options)
    .Then<ValidateOrderBlock>()
    .Then(b => b.Parallel(
        b.ResolveInstance<UpdateInventoryBlock>(),
        b.ResolveInstance<UpdateAnalyticsBlock>(),
        b.ResolveInstance<SendEmailBlock>()
    ))
    .Then<CompleteOrderBlock>();
```

### Loops

Repeat a block for collections:

```csharp
var pipe = builder.CreatePipe(options)
    .Then(b => b.Loop<ProcessOrderItemBlock>(
        parameter => parameter.Value.Items
    ))
    .Then<FinalizeOrderBlock>();
```

### Context Persistence

Maintain state across blocks:

```csharp
public class CalculateShippingBlock : CodeBlock<OrderData>
{
    protected override Parameter<OrderData> Execute(Parameter<OrderData> parameter, OrderData order)
    {
        // Store in context for use by later blocks
        parameter.Context.Set("ShippingCost", 9.99m);
        parameter.Context.Set("EstimatedDelivery", DateTime.UtcNow.AddDays(3));
        
        return parameter;
    }
}

public class ApplyShippingBlock : CodeBlock<OrderData>
{
    protected override Parameter<OrderData> Execute(Parameter<OrderData> parameter, OrderData order)
    {
        // Retrieve from context
        var shippingCost = parameter.Context.Get<decimal>("ShippingCost");
        var delivery = parameter.Context.Get<DateTime>("EstimatedDelivery");
        
        order.Amount += shippingCost;
        order.Status = $"Ready for delivery on {delivery:d}";
        
        return parameter;
    }
}
```

### Async/Await Support

Mix async and sync blocks seamlessly:

```csharp
var result = await pipe.ExecuteAsync(new Parameter<OrderData>(order));
```

---

## Comparison: Traditional vs. MM.PipeBlocks

### Example: Order Processing Workflow

#### ? Traditional Approach
```csharp
public class OrderService
{
    private readonly IValidationService _validation;
    private readonly IInventoryService _inventory;
    private readonly IPaymentService _payment;
    private readonly IShippingService _shipping;
    private readonly INotificationService _notification;
    
    public async Task<OrderResult> ProcessOrderAsync(Order order)
    {
        // Validation with error handling
        var validationErrors = _validation.Validate(order);
        if (validationErrors.Any())
        {
            return OrderResult.Failure(string.Join(", ", validationErrors));
        }
        
        // Inventory check with error handling
        var inventoryAvailable = await _inventory.CheckAvailabilityAsync(order);
        if (!inventoryAvailable)
        {
            return OrderResult.Failure("Insufficient inventory");
        }
        
        // Payment processing with error handling
        PaymentResult paymentResult = null;
        try
        {
            paymentResult = await _payment.ChargeAsync(order.Total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment processing failed");
            return OrderResult.Failure("Payment failed: " + ex.Message);
        }
        
        if (!paymentResult.IsSuccess)
        {
            return OrderResult.Failure("Payment declined");
        }
        
        // Shipment creation with error handling and rollback
        try
        {
            var shipment = await _shipping.CreateShipmentAsync(order);
            if (shipment == null)
            {
                // Manual rollback on failure
                await _payment.RefundAsync(paymentResult.TransactionId);
                return OrderResult.Failure("Shipment creation failed");
            }
        }
        catch (Exception ex)
        {
            await _payment.RefundAsync(paymentResult.TransactionId);
            return OrderResult.Failure("Shipment error: " + ex.Message);
        }
        
        // Notification with separate error handling
        try
        {
            await _notification.SendOrderConfirmationAsync(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notification failed");
            // Don't fail the order if notification fails
        }
        
        return OrderResult.Success(order);
    }
}
```

**Problems:**
- ?? Scattered error handling logic
- ?? Difficult to trace data flow (6+ service calls)
- ?? Manual rollback logic
- ?? Not composable - ProcessOrderAsync is monolithic
- ?? Context (correlation ID, user info) not carried
- ?? Hard to test individual steps
- ?? Defensive coding throughout

#### ? MM.PipeBlocks Approach
```csharp
// Define blocks (each focused on one step)
public class ValidateOrderBlock : CodeBlock<Order>
{
    private readonly IValidationService _validation;
    
    public ValidateOrderBlock(IValidationService validation) => _validation = validation;
    
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        var errors = _validation.Validate(order);
        if (errors.Any())
        {
            parameter.SignalBreak(new DefaultFailureState<Order>(order)
            {
                FailureReason = string.Join(", ", errors)
            });
        }
        return parameter;
    }
}

public class CheckInventoryBlock : AsyncCodeBlock<Order>
{
    private readonly IInventoryService _inventory;
    
    public CheckInventoryBlock(IInventoryService inventory) => _inventory = inventory;
    
    protected override async ValueTask<Parameter<Order>> ExecuteAsync(
        Parameter<Order> parameter, Order order)
    {
        var available = await _inventory.CheckAvailabilityAsync(order);
        if (!available)
        {
            parameter.SignalBreak(new DefaultFailureState<Order>(order)
            {
                FailureReason = "Insufficient inventory"
            });
        }
        return parameter;
    }
}

public class ProcessPaymentBlock : AsyncCodeBlock<Order>
{
    private readonly IPaymentService _payment;
    
    public ProcessPaymentBlock(IPaymentService payment) => _payment = payment;
    
    protected override async ValueTask<Parameter<Order>> ExecuteAsync(
        Parameter<Order> parameter, Order order)
    {
        try
        {
            var result = await _payment.ChargeAsync(order.Total);
            if (!result.IsSuccess)
            {
                parameter.SignalBreak(new DefaultFailureState<Order>(order)
                {
                    FailureReason = "Payment declined"
                });
            }
            else
            {
                // Store transaction ID in context for potential refund
                parameter.Context.Set("TransactionId", result.TransactionId);
            }
        }
        catch (Exception ex)
        {
            parameter.SignalBreak(new DefaultFailureState<Order>(order)
            {
                FailureReason = $"Payment error: {ex.Message}"
            });
        }
        return parameter;
    }
}

public class CreateShipmentBlock : AsyncCodeBlock<Order>
{
    private readonly IShippingService _shipping;
    private readonly IPaymentService _payment;
    
    public CreateShipmentBlock(IShippingService shipping, IPaymentService payment)
    {
        _shipping = shipping;
        _payment = payment;
    }
    
    protected override async ValueTask<Parameter<Order>> ExecuteAsync(
        Parameter<Order> parameter, Order order)
    {
        try
        {
            var shipment = await _shipping.CreateShipmentAsync(order);
            if (shipment == null)
            {
                // Automatic rollback - shipment failed
                var transactionId = parameter.Context.Get<string>("TransactionId");
                await _payment.RefundAsync(transactionId);
                
                parameter.SignalBreak(new DefaultFailureState<Order>(order)
                {
                    FailureReason = "Shipment creation failed"
                });
            }
        }
        catch (Exception ex)
        {
            // Automatic rollback on exception
            var transactionId = parameter.Context.Get<string>("TransactionId");
            await _payment.RefundAsync(transactionId);
            
            parameter.SignalBreak(new DefaultFailureState<Order>(order)
            {
                FailureReason = $"Shipment error: {ex.Message}"
            });
        }
        return parameter;
    }
}

public class SendConfirmationBlock : AsyncCodeBlock<Order>
{
    private readonly INotificationService _notification;
    
    public SendConfirmationBlock(INotificationService notification) => _notification = notification;
    
    protected override async ValueTask<Parameter<Order>> ExecuteAsync(
        Parameter<Order> parameter, Order order)
    {
        // This block only executes if previous blocks succeeded
        try
        {
            await _notification.SendOrderConfirmationAsync(order);
        }
        catch (Exception ex)
        {
            // Log but don't fail the order
            _logger.LogWarning(ex, "Failed to send confirmation");
        }
        return parameter;
    }
}

// Setup and create pipeline
var services = new ServiceCollection();
services.AddTransient<ValidateOrderBlock>();
services.AddTransient<CheckInventoryBlock>();
services.AddTransient<ProcessPaymentBlock>();
services.AddTransient<CreateShipmentBlock>();
services.AddTransient<SendConfirmationBlock>();
// ... register other dependencies ...

var provider = services.BuildServiceProvider();
var builder = new BlockBuilder<Order>(provider.GetRequiredService<ILoggerFactory>());

var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions 
{ 
    PipeName = "Order Processing Pipeline"
}))
    .Then<ValidateOrderBlock>()
    .Then<CheckInventoryBlock>()
    .Then<ProcessPaymentBlock>()
    .Then<CreateShipmentBlock>()
    .Then<SendConfirmationBlock>();

// Execute
var order = new Order { /* ... */ };
var result = pipe.Execute(new Parameter<Order>(order));

// Handle result
result.Match(
    failure =>
    {
        _logger.LogError("Order processing failed: {Reason}", failure.FailureReason);
        return OrderResult.Failure(failure.FailureReason);
    },
    success =>
    {
        _logger.LogInformation("Order processed successfully: {OrderId}", success.OrderId);
        return OrderResult.Success(success);
    }
);
```

**Benefits:**
- ? Each block is a single, testable unit
- ? Unified error handling (same pattern everywhere)
- ? Clear data flow (linear progression)
- ? Reusable blocks in other pipelines
- ? Context automatically carried (correlation ID, state)
- ? Easy to mock dependencies for testing
- ? Happy path coding - failures handled automatically

---

## Comparison Matrix

| Aspect | Traditional | MM.PipeBlocks |
|--------|-----------|---------------|
| **Code Readability** | Nested conditionals, hard to follow | Linear flow, easy to scan |
| **Error Handling** | Scattered try/catch everywhere | Unified two-rail system |
| **Data Flow Traceability** | Lost between service calls | Carried through Parameter & Context |
| **Composability** | Monolithic, hard to reuse | Blocks are fully reusable |
| **Context Management** | Manual passing of context | Automatic via Context object |
| **Testing** | Hard to test individual steps | Easy unit testing of blocks |
| **Code Duplication** | Common patterns repeated | DRY - write validation/error handling once |
| **Rollback Logic** | Manual implementation | Automatic via signaling |
| **Async/Sync Mixing** | Prone to deadlocks | Seamless support |
| **Lines of Code** | 50-100+ for complex flows | 30-50 with clearer intent |

---

## Best Practices

### 1. Keep Blocks Focused
Each block should do one thing well:

```csharp
// ? Good - Single responsibility
public class ValidateOrderBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        if (order.Amount <= 0)
            parameter.SignalBreak(...);
        return parameter;
    }
}

// ? Poor - Multiple responsibilities
public class ProcessOrderBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        // Validating
        if (order.Amount <= 0) parameter.SignalBreak(...);
        
        // Calculating
        var total = CalculateTotal(order);
        
        // Saving to database
        _db.Orders.Add(order);
        
        // Sending notifications
        SendEmail(order);
        
        return parameter;
    }
}
```

### 2. Use Context for Shared State

```csharp
// ? Good - Share state via Context
public class CalculateDiscountBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        var discount = CalculateDiscount(order);
        parameter.Context.Set("Discount", discount);  // Store for later blocks
        order.Total -= discount;
        return parameter;
    }
}

public class ApplyTaxBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        var discount = parameter.Context.Get<decimal>("Discount");  // Retrieve
        var taxableAmount = order.Total - discount;
        order.Tax = taxableAmount * 0.1m;
        return parameter;
    }
}
```

### 3. Leverage Dependency Injection

```csharp
// ? Good - Inject dependencies
public class ProcessPaymentBlock : AsyncCodeBlock<Order>
{
    private readonly IPaymentGateway _gateway;
    private readonly ILogger<ProcessPaymentBlock> _logger;
    
    public ProcessPaymentBlock(
        IPaymentGateway gateway,
        ILogger<ProcessPaymentBlock> logger)
    {
        _gateway = gateway;
        _logger = logger;
    }
    
    protected override async ValueTask<Parameter<Order>> ExecuteAsync(
        Parameter<Order> parameter, Order order)
    {
        _logger.LogInformation("Processing payment for order {OrderId}", order.OrderId);
        try
        {
            var result = await _gateway.ChargeAsync(order.Total);
            // ...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment failed");
            // ...
        }
        return parameter;
    }
}
```

### 4. Use Branching for Conditional Logic

```csharp
// ? Good - Use branching for decisions
var pipe = builder.CreatePipe(options)
    .Then<ValidateOrderBlock>()
    .Then(b => b.Switch(p => p.Value.Amount > 1000
        ? b.ResolveInstance<PremiumProcessingBlock>()
        : b.ResolveInstance<StandardProcessingBlock>()
    ));

// ? Poor - Conditional logic inside block
public class ProcessOrderBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        if (order.Amount > 1000)
        {
            // Premium logic...
        }
        else
        {
            // Standard logic...
        }
        return parameter;
    }
}
```

### 5. Early Failure

Fail fast when preconditions aren't met:

```csharp
// ? Good - Fail early
public class ValidateOrderBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        if (order == null)
        {
            parameter.SignalBreak(new DefaultFailureState<Order>(order)
            {
                FailureReason = "Order is null"
            });
            return parameter;  // Early exit
        }
        
        if (order.Items.Count == 0)
        {
            parameter.SignalBreak(new DefaultFailureState<Order>(order)
            {
                FailureReason = "Order has no items"
            });
            return parameter;  // Early exit
        }
        
        return parameter;  // Success case
    }
}
```

---

## Summary

MM.PipeBlocks transforms how you write business logic in C#:

| From | To |
|------|-----|
| Nested if statements | Linear pipeline flow |
| Service soup | Reusable, testable blocks |
| Scattered error handling | Unified two-rail system |
| Lost context | Preserved via Parameter & Context |
| Hard to test | Easy unit testing |
| Monolithic code | Composable, modular design |

By adopting MM.PipeBlocks, you get cleaner code, better maintainability, improved testability, and a clear, traceable flow through your business logic.

**Start building pipelines today and never go back to nested conditionals!**

---

## Resources

- **GitHub**: https://github.com/mmintoff/PipeBlocks
- **NuGet**: MM.PipeBlocks
- **Examples**: See the `Examples/` directory for complete working samples


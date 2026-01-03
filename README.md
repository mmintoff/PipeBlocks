# MM.PipeBlocks

## Introduction

<p align="center">
  <img src="icon.png" alt="PipeBlocks Logo" width="150" align="left"/>
</p>

**MM.PipeBlocks** is a composable, modular pipeline library for .NET that enables process-oriented programming with sequential execution, branching, error handling, and integrated async/sync support. Encapsulate business logic into reusable "blocks" that chain together to form complex workflows while maintaining clarity, testability, and early failure semantics.

Whether you're processing data through multiple stages, orchestrating microservices, or implementing complex business workflows, MM.PipeBlocks provides a structured, functional approach to handling process flows that eliminates the complexity of deeply nested control structures and scattered error handling.

<p clear="left"/>

## Purpose

Pipeline-oriented programming is an effort to simplify process flows in programming by establishing a mono-directional flow—logic moves in one direction only. This package supports mono-directional flow with branching capabilities, making complex workflows easy to understand, debug, and maintain.

Instead of traditional nested conditional logic where services call other services (creating a tangled web of dependencies and control flow), MM.PipeBlocks enforces a clean, linear progression through discrete, composable steps.

## Why MM.PipeBlocks?

### The Problem: Traditional C# Approaches

Modern C# applications often suffer from several architectural challenges:

#### 1. **Deeply Nested Control Flow**
Traditional approaches often lead to nested `if` statements and branching logic that becomes increasingly difficult to follow:

```csharp
// ❌ Traditional Approach - How it actually grows in real codebases
// Started simple, then requirements kept piling on...
public async Task<bool> ProcessInvoiceAsync(Invoice invoice, User requestingUser)
{
    if (invoice == null) return false;
    if (invoice.Amount == 0) return false;
    if (invoice.Status != "Pending") return false;
    
    if (!await _authService.CanUserApproveAsync(requestingUser, invoice))
    {
        _logger.LogWarning($"User {requestingUser.Id} unauthorized to approve invoice");
        return false;
    }
    
    if (await _invoiceRepository.GetApprovalCountAsync(invoice) < 2)
    {
        await _notificationService.SendNeedsApprovalAsync(invoice);
        return false;
    }
    
    if (await _complianceService.IsRestrictedVendorAsync(invoice.VendorId))
    {
        _logger.LogError($"Vendor {invoice.VendorId} is restricted");
        return false;
    }
    
    if (invoice.Amount > 10000)
    {
        if (!await _approvalService.HasSeniorApprovalAsync(invoice))
        {
            await _notificationService.SendEscalationRequiredAsync(invoice);
            return false;
        }
    }
    
    var payment = await _paymentGateway.ProcessAsync(invoice);
    if (payment == null)
    {
        await _notificationService.SendPaymentFailedAsync(invoice);
        return false;
    }
    
    try
    {
        await _accountingService.RecordTransactionAsync(payment);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Accounting failed");
        // Should we refund? Rollback? Too late now...
        return false;
    }
    
    if (invoice.Amount > 5000)
    {
        await _auditService.LogAsync(new AuditEntry { /* ... */ });
    }
    
    await _invoiceRepository.UpdateStatusAsync(invoice, "Approved");
    await _notificationService.SendApprovedAsync(invoice);
    
    return true;
}
```

**Problems with this approach (the real ones):**
- Mixed concerns scattered throughout: validation, authorization, business rules, payments, accounting, logging, notifications, audit
- Each new requirement adds another branch or nested condition
- Early returns make it hard to see what happens on the happy path
- Inconsistent error handling: some return false, some throw, some log but continue
- No clear separation between "validation failed" and "authorization failed"
- Manual rollback logic would need to be scattered everywhere
- Difficult to test individual concerns in isolation
- When payment fails after accounting records it, you're in an inconsistent state

#### 2. **Service-Calling-Service Complexity**
Tracking data flow becomes nearly impossible when services call other services, which call other services. Context gets lost, and it's hard to understand what transformations occur at each step:

```csharp
// ❌ Service Soup - Hard to track data flow
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
// ❌ Scattered Error Handling
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
// ❌ Tightly Coupled Components
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
// ❌ Async logic forced into a synchronous interface
public Result Process(Order order)
{
    Validate(order);

    // Forced sync wait due to interface constraints
    var pricing = _pricingService
        .GetPricingAsync(order)
        .GetAwaiter()
        .GetResult(); // Can deadlock in certain contexts

    Save(order, pricing);

    _notificationService.SendAsync(order); // Fire-and-forget
    
    return Result.Success();
}
```

#### 6. **Lost Context Across Steps**
As data flows through multiple service calls, contextual information (logging correlation IDs, user context, execution state) is often lost or forgotten:

```csharp
// ❌ Lost Context
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
// ❌ Defensive Everywhere
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

✅ **Clean, Linear Flow** - Execute steps in a single direction  
✅ **Early Failure** - Stop processing on failure and propagate errors  
✅ **Unified Error Handling** - One consistent error handling strategy  
✅ **Fully Composable** - Reuse blocks in different pipelines  
✅ **Mixed Async/Sync** - Handle both seamlessly  
✅ **Preserved Context** - Carry data and context through the entire pipeline  
✅ **Happy Path Coding** - Write for the success case; failures are handled automatically  

---

## Core Concepts

### The Two-Rail System

MM.PipeBlocks implements a two-rail (Either) monad pattern where each step's result is either a **success** (right rail) or a **failure** (left rail). Once a failure occurs, processing stops immediately and the failure state is carried through to the end.

```csharp
// Each step either succeeds or fails
Parameter<OrderData>
    ├─ Success Path ──→ Validate ──→ Process ──→ Confirm ──→ Result
    └─ Failure Path ──→ [STOP] ───────────────→ Return Error
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
// Setup with dependency injection
var services = new ServiceCollection();
services.AddPipeBlocks()
    .AddTransientBlock<ValidateOrderBlock>()
    .AddTransientBlock<ProcessPaymentBlock>();

services.AddTransient<IPaymentGateway, PaymentGateway>();
services.AddLogging();

var provider = services.BuildServiceProvider();
var builder = provider.GetRequiredService<BlockBuilder<Order>>();

// Create the pipeline ONCE - it's reusable and thread-safe
var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions 
{ 
    PipeName = "Order Processing" 
}))
    .Then<ValidateOrderBlock>()
    .Then<ProcessPaymentBlock>();

// Execute the pipeline multiple times with different data
var order1 = new OrderData
{
    OrderId = "ORD-001",
    Amount = 99.99m,
    CustomerEmail = "customer@example.com",
    CreatedAt = DateTime.UtcNow,
    Status = "Created"
};

var result1 = pipe.Execute(new Parameter<OrderData>(order1));

// Same pipeline, different data - no need to recreate it
var order2 = new OrderData
{
    OrderId = "ORD-002",
    Amount = 149.99m,
    CustomerEmail = "another@example.com",
    CreatedAt = DateTime.UtcNow,
    Status = "Created"
};

var result2 = pipe.Execute(new Parameter<OrderData>(order2));

// Handle results using two-rail pattern
foreach (var result in new[] { result1, result2 })
{
    result.Match(
        failure =>
        {
            Console.WriteLine($"❌ Failed: {failure.FailureReason}");
            Console.WriteLine($"   Order: {failure.Value.OrderId}");
        },
        success =>
        {
            Console.WriteLine($"✅ Success: Order {success.OrderId} - {success.Status}");
        }
    );
}
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
    .Then(b => b.Parallelize(
        [
            b.ResolveInstance<UpdateInventoryBlock>(),
            b.ResolveInstance<UpdateAnalyticsBlock>(),
            b.ResolveInstance<SendEmailBlock>()
        ],
        new Join<OrderData>((originalValue, parallelResults) =>
        {
            foreach (var pResult in parallelResults)
            {
                // Merge results onto originalValue if needed
            }
            return originalValue;
        })
    ))
    .Then<CompleteOrderBlock>();
```

### Loops

Repeat a block for collections:

```csharp
var pipe = builder.CreatePipe(options)
    .Then(b => b.Loop()
                .While<ProcessOrderItemBlock>(
                    parameter => parameter.Value.Items > 0
                )
    ))
    .Then<FinalizeOrderBlock>();
```

### Context Persistence

Maintain state across blocks:

```csharp
public class CalculateShippingBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        // Store in context for use by later blocks
        parameter.Context.Set("ShippingCost", 9.99m);
        parameter.Context.Set("EstimatedDelivery", DateTime.UtcNow.AddDays(3));
        
        return parameter;
    }
}

public class ApplyShippingBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
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

Mix async and sync blocks seamlessly in the same pipeline:

```csharp
// Define synchronous blocks
public class ValidateOrderBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        if (order.Amount <= 0)
            parameter.SignalBreak(new DefaultFailureState<Order>(order)
            {
                FailureReason = "Amount must be positive"
            });
        return parameter;
    }
}

public class CalculateTaxBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        order.Tax = order.Amount * 0.1m;  // Sync calculation
        parameter.Context.Set("TaxCalculated", DateTime.UtcNow);
        return parameter;
    }
}

// Define asynchronous blocks
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
                FailureReason = "Out of stock"
            });
        }
        return parameter;
    }
}

public class ProcessPaymentBlock : AsyncCodeBlock<Order>
{
    private readonly IPaymentGateway _gateway;
    
    public ProcessPaymentBlock(IPaymentGateway gateway) => _gateway = gateway;
    
    protected override async ValueTask<Parameter<Order>> ExecuteAsync(
        Parameter<Order> parameter, Order order)
    {
        var result = await _gateway.ChargeAsync(order.Amount + order.Tax);
        if (!result.IsSuccess)
        {
            parameter.SignalBreak(new DefaultFailureState<Order>(order)
            {
                FailureReason = "Payment failed"
            });
        }
        return parameter;
    }
}

// Build a single pipeline with BOTH sync and async blocks mixed together
var services = new ServiceCollection();
services.AddPipeBlocks()
    .AddTransientBlock<ValidateOrderBlock>()      // ← Sync
    .AddTransientBlock<CalculateTaxBlock>()       // ← Sync
    .AddTransientBlock<CheckInventoryBlock>()     // ← Async
    .AddTransientBlock<ProcessPaymentBlock>();   // ← Async

services.AddTransient<IInventoryService, InventoryService>();
services.AddTransient<IPaymentGateway, PaymentGateway>();

var provider = services.BuildServiceProvider();
var builder = provider.GetRequiredService<BlockBuilder<Order>>();

// Create ONE pipeline with mixed sync/async blocks
var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions 
{ 
    PipeName = "Order Processing with Mixed Blocks"
}))
    .Then<ValidateOrderBlock>()        // Sync
    .Then<CalculateTaxBlock>()         // Sync
    .Then<CheckInventoryBlock>()       // Async (pipeline handles seamlessly)
    .Then<ProcessPaymentBlock>();      // Async

// Execute synchronously - MM.PipeBlocks handles the async blocks internally
var order = new Order { Amount = 99.99m, /* ... */ };
var syncResult = pipe.Execute(new Parameter<Order>(order));

syncResult.Match(
    failure => Console.WriteLine($"❌ Failed: {failure.FailureReason}"),
    success => Console.WriteLine($"✅ Success: Order total {success.Amount + success.Tax}")
);

// Execute asynchronously - cleaner async/await flow
var asyncResult = await pipe.ExecuteAsync(new Parameter<Order>(order));

asyncResult.Match(
    failure => Console.WriteLine($"❌ Failed: {failure.FailureReason}"),
    success => Console.WriteLine($"✅ Success: Order total {success.Amount + success.Tax}")
);
```

**Key Benefits:**
- ✅ **One pipeline, multiple execution modes** - Same pipe works with both `Execute()` and `ExecuteAsync()`
- ✅ **Seamless mixing** - Sync blocks don't block async flow or cause deadlocks
- ✅ **No manual orchestration** - MM.PipeBlocks handles sync/async transitions automatically
- ✅ **Clean execution** - Async execution uses proper `await` without `.Result` anti-patterns
- ✅ **Context preserved** - Context carries through both sync and async blocks

### Automatic Exception Handling

By default, MM.PipeBlocks allows exceptions to propagate, but you can optionally configure the pipeline to catch unhandled exceptions automatically and convert them to an `ExceptionFailureState<V>`. This eliminates the need for try/catch blocks in your business logic:

**Without Exception Handling (Manual try/catch):**
```csharp
public class ProcessPaymentBlock : AsyncCodeBlock<OrderData>
{
    private readonly IPaymentGateway _gateway;
    
    protected override async ValueTask<Parameter<OrderData>> ExecuteAsync(
        Parameter<OrderData> parameter, OrderData order)
    {
        try  // ← Manual try/catch required
        {
            var result = await _gateway.ChargeAsync(order.Amount);
            if (!result.IsSuccess)
            {
                parameter.SignalBreak(new DefaultFailureState<OrderData>(order)
                {
                    FailureReason = "Payment declined"
                });
            }
        }
        catch (Exception ex)  // ← Manual exception handling
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

**With Automatic Exception Handling (Clean business logic):**
```csharp
// Configure pipeline to handle exceptions automatically
var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions 
{ 
    PipeName = "Order Processing",
    HandleExceptions = true  // ← Enable automatic exception handling
]))
    .Then<ValidateOrderBlock>()
    .Then<ProcessPaymentBlock>();

// Now blocks can be written without try/catch
public class ProcessPaymentBlock : AsyncCodeBlock<OrderData>
{
    private readonly IPaymentGateway _gateway;
    
    protected override async ValueTask<Parameter<OrderData>> ExecuteAsync(
        Parameter<OrderData> parameter, OrderData order)
    {
        // No try/catch needed - exceptions are caught by the pipeline
        var result = await _gateway.ChargeAsync(order.Amount);
        
        if (!result.IsSuccess)
        {
            parameter.SignalBreak(new DefaultFailureState<OrderData>(order)
            {
                FailureReason = "Payment declined"
            });
        }
        
        return parameter;
    }
}

// Any unhandled exception becomes an ExceptionFailureState<V>
var result = pipe.Execute(new Parameter<OrderData>(order));

result.Match(
    failure =>
    {
        // Handle both business logic failures and exceptions the same way
        if (failure is ExceptionFailureState<OrderData> exFailure)
        {
            Console.WriteLine($"❌ Unexpected error: {exFailure.Exception.Message}");
        }
        else
        {
            Console.WriteLine($"❌ Business logic failed: {failure.FailureReason}");
        }
    },
    success => Console.WriteLine($"✅ Success: Order {success.OrderId}")
);
```

**Benefits of Automatic Exception Handling:**
- ✅ Write clean business logic without defensive try/catch blocks
- ✅ Unhandled exceptions automatically stop the pipeline (fail fast)
- ✅ All failures (business logic and exceptions) follow the same two-rail pattern
- ✅ Exception details preserved in `ExceptionFailureState<V>` for logging and debugging
- ✅ No need to manually decide what to do when unexpected errors occur

---

## Comparison: Traditional vs. MM.PipeBlocks

### Example: Order Processing Workflow

#### ❌ Traditional Approach
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
- Scattered error handling logic
- Difficult to trace data flow (6+ service calls)
- Manual rollback logic
- Not composable - ProcessOrderAsync is monolithic
- Context (correlation ID, user info) not carried
- Hard to test individual steps
- Defensive coding throughout

#### ✅ MM.PipeBlocks Approach
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
- ✅ Each block is a single, testable unit
- ✅ Unified error handling (same pattern everywhere)
- ✅ Clear data flow (linear progression)
- ✅ Reusable blocks in other pipelines
- ✅ Context automatically carried (correlation ID, state)
- ✅ Easy to mock dependencies for testing
- ✅ Happy path coding - failures handled automatically

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
// ✅ Good - Single responsibility
public class ValidateOrderBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        if (order.Items.Count == 0)
            parameter.SignalBreak(new DefaultFailureState<Order>(order) 
            { 
                FailureReason = "No items" 
            });
        return parameter;
    }
}

// ❌ Poor - Multiple responsibilities
public class ProcessOrderBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        // Validating
        if (order.Items.Count == 0) parameter.SignalBreak(...);
        
        // Calculating
        var total = CalculateTotal(order);
        
        // Saving to database
        _db.Orders.Add(order);
        
        // Sending notifications
        SendEmail(order);
        
        // All tightly coupled - hard to reuse parts independently
    }
}
```

### 2. Use Context for Shared State

```csharp
// ✅ Good - Share state via Context
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
// ✅ Good - Inject dependencies
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
// ✅ Good - Use branching for decisions
var pipe = builder.CreatePipe(options)
    .Then<ValidateOrderBlock>()
    .Then(b => b.Switch(p => p.Value.Amount > 1000
        ? b.ResolveInstance<PremiumProcessingBlock>()
        : b.ResolveInstance<StandardProcessingBlock>()
    ))
    .Then<SendConfirmationBlock>();

// ❌ Poor - Conditional logic inside block
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
// ✅ Good - Fail early
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

## SOLID Principles & MM.PipeBlocks

MM.PipeBlocks naturally aligns with SOLID design principles, making it easier to build maintainable, testable, and scalable applications. Here's how:

### **S - Single Responsibility Principle**

> A class should have only one reason to change.

**Traditional Approach - Violates SRP:**
```csharp
// ❌ Multiple responsibilities
public class OrderProcessor
{
    // Responsible for: validation, payment, inventory, shipping, notifications
    public async Task<bool> ProcessOrderAsync(Order order)
    {
        // Validation logic
        if (order.Items.Count == 0) return false;
        
        // Payment logic
        var paid = await _paymentGateway.ChargeAsync(order.Total);
        if (!paid) return false;
        
        // Inventory logic
        await _inventory.ReserveAsync(order);
        
        // Shipping logic
        var shipment = await _shipping.CreateAsync(order);
        
        // Notification logic
        await _notifications.SendAsync(order);
        
        return true;
    }
}
```

The `OrderProcessor` class has 5 reasons to change: validation rules, payment processing, inventory management, shipping logic, or notification requirements.

**MM.PipeBlocks - Honors SRP:**
```csharp
// ✅ Each block has ONE reason to change

public class ValidateOrderBlock : CodeBlock<Order>
{
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        if (order.Items.Count == 0)
            parameter.SignalBreak(new DefaultFailureState<Order>(order) 
            { 
                FailureReason = "No items" 
            });
        return parameter;
    }
}

public class ProcessPaymentBlock : AsyncCodeBlock<Order>
{
    private readonly IPaymentGateway _gateway;
    
    protected override async ValueTask<Parameter<Order>> ExecuteAsync(
        Parameter<Order> parameter, Order order)
    {
        var result = await _gateway.ChargeAsync(order.Total);
        if (!result.IsSuccess)
            parameter.SignalBreak(new DefaultFailureState<Order>(order) 
            { 
                FailureReason = "Payment failed" 
            });
        return parameter;
    }
}

public class ReserveInventoryBlock : AsyncCodeBlock<Order>
{
    private readonly IInventoryService _inventory;
    
    protected override async ValueTask<Parameter<Order>> ExecuteAsync(
        Parameter<Order> parameter, Order order)
    {
        await _inventory.ReserveAsync(order);
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

// Each block has exactly **one reason to change**: when its specific business logic changes.

---

### **O - Open/Closed Principle**

> Software entities should be open for extension but closed for modification.

**Traditional Approach - Violates OCP:**
```csharp
// ❌ To add fraud detection, must modify existing method
public async Task<bool> ProcessOrderAsync(Order order)
{
    if (order.Items.Count == 0) return false;
    
    // Payment logic
    var paid = await _paymentGateway.ChargeAsync(order.Total);
    if (!paid) return false;
    
    // NEW REQUIREMENT: Add fraud check - Must modify this method
    var isFraudulent = await _fraudDetection.CheckAsync(order);
    if (isFraudulent)
    {
        await _notifications.SendAsync(order);
        return false;
    }
    
    // ... rest of logic
    return true;
}
```

Adding new requirements forces modification of existing code, risking bugs in tested logic.

**MM.PipeBlocks - Honors OCP:**
```csharp
// ✅ Extend pipeline without modifying existing code

// Original pipeline (unchanged, tested, safe)
var pipe = builder.CreatePipe(options)
    .Then<ValidateOrderBlock>()
    .Then<ProcessPaymentBlock>()
    .Then<CreateShipmentBlock>();

// NEW REQUIREMENT: Add fraud detection
// Just insert a new block - no modifications needed
var enhancedPipe = builder.CreatePipe(options)
    .Then<ValidateOrderBlock>()
    .Then<FraudDetectionBlock>()        // ← NEW: Extension, not modification
    .Then<ProcessPaymentBlock>()
    .Then<CreateShipmentBlock>();
```

New blocks extend functionality **without touching existing code**. The system is **closed for modification** but **open for extension** through new blocks.

---

### **L - Liskov Substitution Principle**

> Derived classes must be substitutable for their base classes.

**Traditional Approach - Violates LSP:**
```csharp
// ❌ Different payment methods don't follow same contract
public class PaymentProcessor
{
    public async Task<bool> ProcessAsync(Order order, IPaymentMethod payment)
    {
        if (payment is CreditCard cc)
        {
            // Special handling for credit cards
            var result = await _creditCardProcessor.ProcessAsync(cc, order.Total);
            return result.IsSuccess;
        }
        else if (payment is BankTransfer bt)
        {
            // Different handling for bank transfers
            var result = await _bankTransferProcessor.ProcessAsync(bt, order.Total);
            // Returns different type!
            return result != null;
        }
        else if (payment is PayPal pp)
        {
            // Yet another different approach
            await _paypalService.ProcessAsync(pp.Email, order.Total);
            return true;
        }
        
        return false;
    }
}
```

Each payment type requires different handling and returns different types of results. They're not truly substitutable.

**MM.PipeBlocks - Honors LSP:**
```csharp
// ✅ All payment blocks follow same contract
public abstract class PaymentBlock : AsyncCodeBlock<Order>
{
    protected abstract Task<PaymentResult> ProcessPaymentAsync(Order order);
    
    protected override async ValueTask<Parameter<Order>> ExecuteAsync(
        Parameter<Order> parameter, Order order)
    {
        var result = await ProcessPaymentAsync(order);
        if (!result.IsSuccess)
        {
            parameter.SignalBreak(new DefaultFailureState<Order>(order) 
            { 
                FailureReason = "Payment failed" 
            });
        }
        else
        {
            parameter.Context.Set("TransactionId", result.TransactionId);
        }
        return parameter;
    }
}

public class CreditCardPaymentBlock : PaymentBlock
{
    private readonly ICreditCardProcessor _processor;
    
    protected override Task<PaymentResult> ProcessPaymentAsync(Order order)
        => _processor.ProcessAsync(order);
}

public class BankTransferPaymentBlock : PaymentBlock
{
    private readonly IBankTransferProcessor _processor;
    
    protected override Task<PaymentResult> ProcessPaymentAsync(Order order)
        => _processor.ProcessAsync(order);
}

public class PayPalPaymentBlock : PaymentBlock
{
    private readonly IPayPalService _service;
    
    protected override Task<PaymentResult> ProcessPaymentAsync(Order order)
        => _service.ProcessAsync(order);
}

// All are perfectly substitutable - same interface, same behavior
var pipe = builder.CreatePipe(options)
    .Then<ValidateOrderBlock>()
    .Then(b => paymentMethod switch
    {
        PaymentMethod.CreditCard => b.ResolveInstance<CreditCardPaymentBlock>(),
        PaymentMethod.BankTransfer => b.ResolveInstance<BankTransferPaymentBlock>(),
        PaymentMethod.PayPal => b.ResolveInstance<PayPalPaymentBlock>(),
        _ => throw new NotSupportedException()
    })
    .Then<CreateShipmentBlock>();
```

All payment blocks are true substitutes for each other—they implement the same contract and behavior.

---

### **I - Interface Segregation Principle**

> Clients should not depend on interfaces they don't use.

**Traditional Approach - Violates ISP:**
```csharp
// ❌ Monolithic interface with methods not all clients need
public interface IOrderService
{
    Task<bool> ValidateAsync(Order order);
    Task<PaymentResult> ProcessPaymentAsync(Order order);
    Task<Inventory> ReserveInventoryAsync(Order order);
    Task<Shipment> CreateShipmentAsync(Order order);
    Task SendNotificationAsync(Order order);
    Task LogAuditAsync(Order order);
    Task<TaxCalculation> CalculateTaxAsync(Order order);
    Task UpdateDatabaseAsync(Order order);
}

// ❌ Every implementation must implement all methods
public class OrderService : IOrderService
{
    public Task<bool> ValidateAsync(Order order) { /* ... */ }
    public Task<PaymentResult> ProcessPaymentAsync(Order order) { /* ... */ }
    public Task<Inventory> ReserveInventoryAsync(Order order) { /* ... */ }
    public Task<Shipment> CreateShipmentAsync(Order order) { /* ... */ }
    public Task SendNotificationAsync(Order order) { /* ... */ }
    public Task LogAuditAsync(Order order) { /* ... */ }
    public Task<TaxCalculation> CalculateTaxAsync(Order order) { /* ... */ }
    public Task UpdateDatabaseAsync(Order order) { /* ... */ }
}
```

Clients importing `IOrderService` might only need validation, but they're coupled to all these unrelated operations.

**MM.PipeBlocks - Honors ISP:**
```csharp
// ✅ Each block depends only on interfaces it needs

public class ValidateOrderBlock : CodeBlock<Order>
{
    private readonly IOrderValidator _validator;  // Only one interface
    
    public ValidateOrderBlock(IOrderValidator validator)
        => _validator = validator;
    
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        // Only uses validator
        var errors = _validator.Validate(order);
        if (errors.Any())
            parameter.SignalBreak(new DefaultFailureState<Order>(order) 
            { 
                FailureReason = string.Join(", ", errors) 
            });
        return parameter;
    }
}

public class ProcessPaymentBlock : AsyncCodeBlock<Order>
{
    private readonly IPaymentGateway _gateway;  // Only payment interface
    
    public ProcessPaymentBlock(IPaymentGateway gateway)
        => _gateway = gateway;
    
    protected override async ValueTask<Parameter<Order>> ExecuteAsync(
        Parameter<Order> parameter, Order order)
    {
        // Only uses payment gateway
        var result = await _gateway.ChargeAsync(order.Total);
        if (!result.IsSuccess)
            parameter.SignalBreak(new DefaultFailureState<Order>(order) 
            { 
                FailureReason = "Payment failed" 
            });
        return parameter;
    }
}
```

Each block declares only the interfaces it actually needs—no fat interfaces, no unnecessary coupling.

---

### **D - Dependency Inversion Principle**

> High-level modules should not depend on low-level modules; both should depend on abstractions.

**The Challenge:**
In traditional approaches, even when following DIP, the orchestration layer often becomes a "God Conductor" that knows about all the abstractions and orchestrates them together. This creates coupling between the orchestrator and every service interface.

**Traditional Approach - Orchestrator Coupled to Many Abstractions:**
```csharp
// Even with proper DI, the orchestrator is tightly coupled to many interfaces
public class OrderOrchestrator
{
    private readonly IOrderValidator _validator;
    private readonly IInventoryService _inventory;
    private readonly IPaymentGateway _payment;
    private readonly IShippingProvider _shipping;
    private readonly INotificationService _notifications;
    
    public OrderOrchestrator(
        IOrderValidator validator,
        IInventoryService inventory,
        IPaymentGateway payment,
        IShippingProvider shipping,
        INotificationService notifications)
    {
        _validator = validator;
        _inventory = inventory;
        _payment = payment;
        _shipping = shipping;
        _notifications = notifications;
    }
    
    public async Task<bool> ProcessAsync(Order order)
    {
        if (!_validator.Validate(order)) return false;
        if (!await _inventory.ReserveAsync(order)) return false;
        if (!await _payment.ChargeAsync(order.Total)) return false;
        if (!await _shipping.CreateAsync(order)) return false;
        await _notifications.SendAsync(order);
        return true;
    }
}

// The orchestrator has hard dependencies on 5 different service interfaces
// Adding a new step means modifying the constructor and the orchestrator logic
```

While this follows DIP (depends on abstractions), the orchestrator is **coupled to the structure and interfaces of every service**. Adding a fraud check means:
1. Adding a new constructor parameter
2. Storing it as a field
3. Calling it in the right place in the method
4. Modifying and retesting the entire orchestrator

**MM.PipeBlocks - Decouples the Orchestrator from Individual Services:**
```csharp
// Each block is independently responsible for its concerns
public class ValidateOrderBlock : CodeBlock<Order>
{
    private readonly IOrderValidator _validator;
    
    public ValidateOrderBlock(IOrderValidator validator)
        => _validator = validator;
    
    protected override Parameter<Order> Execute(Parameter<Order> parameter, Order order)
    {
        if (!_validator.Validate(order))
            parameter.SignalBreak(new DefaultFailureState<Order>(order) 
            { 
                FailureReason = "Validation failed" 
            });
        return parameter;
    }
}

public class ReserveInventoryBlock : AsyncCodeBlock<Order>
{
    private readonly IInventoryService _inventory;
    
    public ReserveInventoryBlock(IInventoryService inventory)
        => _inventory = inventory;
    
    protected override async ValueTask<Parameter<Order>> ExecuteAsync(
        Parameter<Order> parameter, Order order)
    {
        var reserved = await _inventory.ReserveAsync(order);
        if (!reserved)
            parameter.SignalBreak(new DefaultFailureState<Order>(order) 
            { 
                FailureReason = "Inventory unavailable" 
            });
        return parameter;
    }
}

public class ProcessPaymentBlock : AsyncCodeBlock<Order>
{
    private readonly IPaymentGateway _payment;
    
    public ProcessPaymentBlock(IPaymentGateway payment)
        => _payment = payment;
    
    protected override async ValueTask<Parameter<Order>> ExecuteAsync(
        Parameter<Order> parameter, Order order)
    {
        var result = await _payment.ChargeAsync(order.Total);
        if (!result.IsSuccess)
            parameter.SignalBreak(new DefaultFailureState<Order>(order) 
            { 
                FailureReason = "Payment failed" 
            });
        return parameter;
    }
}

// The pipeline is the orchestrator - it doesn't know or care about implementations
var pipe = builder.CreatePipe(options)
    .Then<ValidateOrderBlock>()
    .Then<ReserveInventoryBlock>()
    .Then<ProcessPaymentBlock>()
    .Then<CreateShipmentBlock>()
    .Then<SendNotificationBlock>();

// Setup with dependency injection
var services = new ServiceCollection();
services.AddPipeBlocks()
    .AddTransientBlock<ValidateOrderBlock>()
    .AddTransientBlock<ReserveInventoryBlock>()
    .AddTransientBlock<ProcessPaymentBlock>()
    .AddTransientBlock<CreateShipmentBlock>()
    .AddTransientBlock<SendNotificationBlock>();

services.AddTransient<IOrderValidator, OrderValidator>();
services.AddTransient<IInventoryService, InventoryService>();
services.AddTransient<IPaymentGateway, PaymentGateway>();
services.AddTransient<IShippingProvider, ShippingProvider>();
services.AddTransient<INotificationService, NotificationService>();

var provider = services.BuildServiceProvider();
var builder = new BlockBuilder<Order>(provider.GetRequiredService<ILoggerFactory>());

var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions 
{ 
    PipeName = "Order Processing Pipeline"
}))
    .Then<ValidateOrderBlock>()
    .Then<ReserveInventoryBlock>()
    .Then<ProcessPaymentBlock>()
    .Then<CreateShipmentBlock>()
    .Then<SendNotificationBlock>();

// Reuse the same pipeline instance for multiple executions
var order1 = new Order { /* ... */ };
var result1 = pipe.Execute(new Parameter<Order>(order1));

var order2 = new Order { /* ... */ };
var result2 = pipe.Execute(new Parameter<Order>(order2));

// Works with async too
var order3 = new Order { /* ... */ };
var result3 = await pipe.ExecuteAsync(new Parameter<Order>(order3));
```

**Key Differences:**

| Aspect | Traditional | MM.PipeBlocks |
|--------|-----------|---------------|
| **Where Coupling Happens** | Orchestrator coupled to all service interfaces | Each block coupled only to its own interface |
| **Adding a New Step** | Modify orchestrator constructor + method | Create new block, add to pipeline (no existing code changes) |
| **Testing** | Must mock all 5 services to test orchestrator | Test each block independently with just one mock |
| **Extension Point** | Orchestrator is the extension point (violates OCP) | Pipeline builder is the extension point (honors OCP) |
| **Single Responsibility** | Orchestrator has N+1 reasons to change (one per service) | Each block has 1 reason to change |

MM.PipeBlocks doesn't invent DIP—it distributes the dependency burden across many small, focused classes rather than concentrating it in one orchestrator. This makes the system **easier to extend, test, and maintain** without modifying existing code.

---

## Performance Overview

Benchmarks were run using **BenchmarkDotNet v0.15.0** on **.NET 10.0** to measure the overhead introduced by `PipeBlocks` compared to plain C# execution.

While `PipeBlocks` does introduce additional cost, the absolute overhead remains small (hundreds of nanoseconds) and predictable across scenarios. For most real-world workloads, this overhead is negligible relative to I/O, serialization, or business logic costs.

### Key Takeaways

- **Happy-path overhead:** ~180–185 ns  
- **Bad-response overhead:** ~170–180 ns  
- **Worst-case exception path:** ~3–5 µs  
- **Enabling `HandleExceptions` reduces exception cost and allocations**
- **Allocation overhead:** ~480–824 bytes per invocation  
- **No hidden contention or threading costs**
- Large ratios are a microbenchmark artifact; **absolute cost remains very small**

---

## Benchmark Results (Selected)

### Happy Path (No Exceptions)

#### HandleExceptions = false

| Method      | Mean (ns) | Ratio | Allocated |
|-------------|----------:|------:|----------:|
| PlainCSharp | 12.33     | 1.00  | 96 B      |
| PipeBlocks  | 191.92    | 15.57 | 824 B     |

> **~180 ns absolute overhead**  
> ≈ **0.00000018 seconds per call**

---

#### HandleExceptions = true

| Method      | Mean (ns) | Ratio | Allocated |
|-------------|----------:|------:|----------:|
| PlainCSharp | 12.22     | 1.00  | 96 B      |
| PipeBlocks  | 196.70    | 16.10 | 568 B     |

> Enabling exception handling adds **negligible additional cost** on the happy path.

---

### Exception Thrown

#### HandleExceptions = false

| Method      | Mean (ns) | Ratio | Allocated |
|-------------|----------:|------:|----------:|
| PlainCSharp | 1,276.02  | 1.00  | 416 B     |
| PipeBlocks  | 4,710.82  | 3.69  | 2,416 B   |

---

#### HandleExceptions = true

| Method      | Mean (ns) | Ratio | Allocated |
|-------------|----------:|------:|----------:|
| PlainCSharp | 1,274.94  | 1.00  | 416 B     |
| PipeBlocks  | 3,137.55  | 2.46  | 1,448 B   |

> Exception paths incur additional runtime cost due to stack unwinding, allocation, and dispatch.  
> Enabling `HandleExceptions` **significantly reduces both execution time and allocations**, keeping execution within **~3–5 µs**.

---

### Invalid / Bad Response

#### HandleExceptions = false

| Method      | Mean (ns) | Ratio | Allocated |
|-------------|----------:|------:|----------:|
| PlainCSharp | 11.96     | 1.00  | 96 B      |
| PipeBlocks  | 179.72    | 15.04 | 736 B     |

---

#### HandleExceptions = true

| Method      | Mean (ns) | Ratio | Allocated |
|-------------|----------:|------:|----------:|
| PlainCSharp | 11.36     | 1.00  | 96 B      |
| PipeBlocks  | 182.40    | 16.05 | 480 B     |

> Absolute overhead remains **~170–180 ns**, regardless of exception handling mode.

---

## How PipeBlocks Compares to Other .NET Frameworks and Patterns

Raw performance numbers are most useful when placed in context. The following comparisons help position the overhead of `PipeBlocks` relative to other abstractions commonly used in .NET applications.

### Manual C# Control Flow

Plain C# represents the lowest possible overhead, as there is no abstraction layer involved. The happy-path baseline in these benchmarks is ~12 ns.

This serves as a reference point rather than a realistic target for structured application code.

---

### async/await State Machines

`async`/`await` introduces a compiler-generated state machine and continuation scheduling.

- A single completed `await` typically costs **~100–300 ns**
- Incomplete awaits or context switches cost more

The `PipeBlocks` happy-path overhead (~180–200 ns) falls within the same order of magnitude as a single async continuation.

---

### MediatR

MediatR provides in-process request/response and notification dispatch with pipeline behaviors.

Community benchmarks and profiling generally show:

- **~500 ns to 2 µs per request** for simple handlers
- Additional cost per pipeline behavior
- Allocations from request envelopes and handler resolution

Relative to MediatR:

- `PipeBlocks` has **lower per-invocation overhead** on the happy path
- No DI-based handler resolution per call
- No reflection or dynamic dispatch during execution

Both tools target different problem spaces, but from a raw execution perspective, `PipeBlocks` sits below MediatR in overhead.

---

### System.Threading.Channels

Channels are optimized for concurrent producer/consumer scenarios.

- Posting and reading often costs **hundreds of nanoseconds**
- Allocations depend on configuration (bounded vs unbounded, pooling)

`PipeBlocks` has comparable overhead for single-step execution, but without thread coordination or scheduling costs.

---

### TPL Dataflow

TPL Dataflow provides rich dataflow primitives and scheduling options.

Typical characteristics:

- **~500–1500+ ns per block invocation**
- Higher allocation rates unless pooling is carefully configured

Compared to TPL Dataflow, `PipeBlocks` prioritizes simpler execution with lower overhead for linear pipelines.

---

### Reactive Extensions (Rx.NET)

Rx.NET implements push-based observable pipelines.

- Simple operator chains often incur **~200–400 ns per element**
- More complex operators can add significant overhead

`PipeBlocks` overhead is comparable to basic Rx operator chains, without requiring a subscription-based model.

---

### External Message Brokers

External systems such as Kafka, RabbitMQ, or Azure Service Bus introduce:

- Network latency (milliseconds)
- Serialization and batching costs

In these environments, the overhead of in-process abstractions like `PipeBlocks` is negligible by comparison.

---

## Summary Table

| Pattern / Framework        | Typical Overhead (approx) | Notes |
|----------------------------|---------------------------|-------|
| Plain C#                  | ~10–20 ns                 | Baseline |
| async/await (single hop)  | ~100–300 ns               | State machine |
| PipeBlocks (happy path)   | ~180–200 ns               | Structured control flow |
| Channels                  | ~200–600+ ns              | Concurrency-focused |
| Rx.NET                    | ~200–400+ ns              | Push-based |
| MediatR                   | ~500 ns–2 µs+             | DI + pipelines |
| TPL Dataflow              | ~500–1500+ ns             | General-purpose dataflow |
| External brokers           | ms+                       | I/O dominates |

---

## Interpretation

The overhead of `PipeBlocks` is consistent with other mainstream .NET abstractions that trade a small, fixed per-call cost for structure, composability, and correctness.

For applications involving:

- I/O
- Async boundaries
- Serialization
- Validation
- Logging
- Dependency injection

…the relative cost of `PipeBlocks` becomes insignificant. In tighter loops, its overhead remains in the **hundreds of nanoseconds**, not milliseconds.

---

## Conclusion

`PipeBlocks` introduces a small and predictable execution cost that is comparable to, or lower than, many established .NET frameworks used in production systems.

It is best suited for scenarios where clear control flow, composability, and centralized handling outweigh the need for absolute minimal instruction-level overhead.

---

## Acknowledgments

MM.PipeBlocks stands on the shoulders of proven functional programming concepts and excellent supporting libraries:

- **Railway-Oriented Programming** - The two-rail success/failure pattern is inspired by [Scott Wlaschin's excellent work](https://fsharpforfunandprofit.com/rop/) on functional error handling in F#. This paradigm elegantly separates happy path logic from error handling.

- **Either Monad Pattern** - The Either/Result pattern comes from functional programming languages like Haskell and F#, providing a type-safe way to represent success or failure without exceptions.

- **Nito.AsyncEx** - We leverage [Stephen Cleary's Nito.AsyncEx.Context](https://github.com/StephenCleary/AsyncEx) library for safe async/sync context bridging, helping prevent deadlocks in mixed async/sync scenarios.

- **Functional Programming Community** - Concepts from languages like F#, Haskell, and the broader functional programming community have deeply influenced this design.

---

## Resources

- **GitHub**: https://github.com/mmintoff/PipeBlocks
- **NuGet**: MM.PipeBlocks
- **Examples**: See the `Examples/` directory for complete working samples
- **Railway-Oriented Programming**: https://fsharpforfunandprofit.com/rop/
- **Nito.AsyncEx**: https://github.com/StephenCleary/AsyncEx
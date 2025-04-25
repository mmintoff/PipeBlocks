# PipeBlocks
A composable pipeline library for defining process flows with sequential execution, branching, try/catch handling, and mixed sync/async support. Each process step is encapsulated as a "block," enabling modular and unit-testable workflows. The pipeline follows a two-rail system, breaking early on failure, with results wrapped in an Either monad for clear success/failure state management.

## Purpose
Pipeline-oriented programming is an effort to simplify the process flows in programming by having the process flow in one direction only. This package supports mono-directional flow with branching.

## Features
- *Modular Design*: Encapsulate functionality within individual blocks, promoting reusability and testability.
- *Composable Pipelines*: Chain blocks together to define complex workflows in a linear and understandable fashion.
- *Enhanced Readability*: By structuring code into blocks, the overall logic becomes more transparent and easier to follow.

# Getting Started

## Installation
Clone or include the project into your solution
```bash
git clone https://github.com/mmintoff/PipeBlocks.git
```

## Define a Context, Value and FailureState model
```C#
public class MyValueModel
{
    public DateTime RetrievedAt { get; set; }
    public string? TextRetrieved { get; set; }
    public DateTime ProcessedAt { get; set; }
    public int WordCount { get; set; }
}

public class MyContextModel(MyValueModel value) : IContext<MyValueModel>
{
    public Guid CorrelationId { get; set; }
    public Either<IFailureState<MyValueModel>, MyValueModel> Value { get; set; } = value;
    public bool IsFinished { get; set; }
    public bool IsFlipped { get; set; }

    public string? RequestUrl { get; set; }
}

public class MyFailureState : IFailureState<MyValueModel>
{
    public MyValueModel Value { get; set; }
    public Guid CorrelationId { get; set; }
    public string? FailureReason { get; set; }
}
```

## Define Code Blocks
```C#
public class RetrieveTextBlockAsync : AsyncCodeBlock<MyContextModel, MyValueModel>
{
    protected override async ValueTask<MyContextModel> ExecuteAsync(MyContextModel context, MyValueModel value)
    {
        try
        {
            value.TextRetrieved = await new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip
            }).GetStringAsync(context.RequestUrl);
            value.RetrievedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            context.IsFinished = true;
            context.Value = new MyFailureState
            {
                Value = value,
                CorrelationId = context.CorrelationId,
                FailureReason = ex.Message
            };
        }
        return context;
    }
}

public class WordCountBlock : CodeBlock<MyContextModel, MyValueModel>
{
    protected override MyContextModel Execute(MyContextModel context, MyValueModel value)
    {
        char[] tokens = [.. value.TextRetrieved!.Select(c => char.IsLetter(c) ? c : ' ')];
        value.WordCount = new string(tokens).Split([' '], StringSplitOptions.RemoveEmptyEntries).Length;
        value.ProcessedAt = DateTime.UtcNow;
        return context;
    }
}
```

## Define a Block Resolver
```C#
public class ServiceProviderBackedResolver<C, V>(IServiceProvider hostProvider) : IBlockResolver<C, V>
    where C : IContext<V>
{
    public X ResolveInstance<X>() where X : IBlock<C, V>
        => hostProvider.GetRequiredService<X>();
}
```

## Setting up Dependency Injection
```C#
var serviceCollection = new ServiceCollection();
serviceCollection.AddTransient<IBlockResolver<MyContextModel, MyValueModel>, ServiceProviderBackedResolver<MyContextModel, MyValueModel>>();
serviceCollection.AddTransient<BlockBuilder<MyContextModel, MyValueModel>>();
serviceCollection.AddTransient<RetrieveTextBlockAsync>();
serviceCollection.AddTransient<WordCountBlock>();
serviceCollection.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.AddConsole();
    configure.SetMinimumLevel(LogLevel.Trace);
});

var serviceProvider = serviceCollection.BuildServiceProvider();
```

## Create a Pipe
```C#
var builder = serviceProvider.GetRequiredService<BlockBuilder<MyContextModel, MyValueModel>>();
var pipe = builder.CreatePipe("Word Counter")
    .Then(builder.ReturnIf(
        condition: (c, v) => string.IsNullOrWhiteSpace(c.RequestUrl),
        action: (c, v) =>
        {
            c.Value = new MyFailureState
            {
                CorrelationId = c.CorrelationId,
                Value = v,
                FailureReason = "Request Url empty"
            };
        }))
    .Then<RetrieveTextBlockAsync>()
    .Then(builder.Run((c, v) => WriteToConsole(v.RetrievedAt)))
    .Then<WordCountBlock>()
    .Then(builder.Run((c, v) => WriteToConsole(v.ProcessedAt)))
    ;
	
void WriteToConsole(DateTime dt)
{
    Console.WriteLine(dt.ToString("yyyyMMdd HHmmss"));
}
```

## Execute Pipe (Expected Failure)
```C#
var result = pipe.Execute(new MyContextModel(new())
{
    CorrelationId = Guid.NewGuid(),
    RequestUrl = null
});

result.Value.Match(
    failure => Console.WriteLine($"Failure: {failure.FailureReason}"),
    success => Console.WriteLine($"Success: {success.WordCount} words"));
```

```bash
info: MM.PipeBlocks.PipeBlock[0]
      Created pipe: 'Word Counter'
trce: MM.PipeBlocks.PipeBlock[0]
      Added block: 'MM.PipeBlocks.Blocks.BranchBlock`2[pbTest.MyContextModel,pbTest.MyValueModel]' to pipe: 'Word Counter'
trce: MM.PipeBlocks.PipeBlock[0]
      Added block: 'pbTest.RetrieveTextBlockAsync' to pipe: 'Word Counter'
trce: MM.PipeBlocks.PipeBlock[0]
      Added block: 'MM.PipeBlocks.Blocks.FuncBlock`2[pbTest.MyContextModel,pbTest.MyValueModel]' to pipe: 'Word Counter'
trce: MM.PipeBlocks.PipeBlock[0]
      Added block: 'pbTest.WordCountBlock' to pipe: 'Word Counter'
trce: MM.PipeBlocks.PipeBlock[0]
      Added block: 'MM.PipeBlocks.Blocks.FuncBlock`2[pbTest.MyContextModel,pbTest.MyValueModel]' to pipe: 'Word Counter'
trce: MM.PipeBlocks.PipeBlock[0]
      Executing pipe: 'Word Counter' synchronously for context: 1f721e87-4f81-4185-ad9a-a236a3f90ad9
info: MM.PipeBlocks.Blocks.ReturnBlock[0]
      Context 1f721e87-4f81-4185-ad9a-a236a3f90ad9 terminated in Return Block
trce: MM.PipeBlocks.PipeBlock[0]
      Stopping synchronous pipe: 'Word Counter' execution at step: 1 for context: 1f721e87-4f81-4185-ad9a-a236a3f90ad9
trce: MM.PipeBlocks.PipeBlock[0]
      Completed synchronous pipe: 'Word Counter' execution for context: 1f721e87-4f81-4185-ad9a-a236a3f90ad9
Failure: Request Url empty
```

## Execute Pipe (Expected Success)
```C#
var result = pipe.Execute(new MyContextModel(new())
{
    CorrelationId = Guid.NewGuid(),
    RequestUrl = "https://www.gutenberg.org/cache/epub/11/pg11.txt"
});

result.Value.Match(
    failure => Console.WriteLine($"Failure: {failure.FailureReason}"),
    success => Console.WriteLine($"Success: {success.WordCount} words"));
```

```bash
info: MM.PipeBlocks.PipeBlock[0]
      Created pipe: 'Word Counter'
trce: MM.PipeBlocks.PipeBlock[0]
      Added block: 'MM.PipeBlocks.Blocks.BranchBlock`2[pbTest.MyContextModel,pbTest.MyValueModel]' to pipe: 'Word Counter'
trce: MM.PipeBlocks.PipeBlock[0]
      Added block: 'pbTest.RetrieveTextBlockAsync' to pipe: 'Word Counter'
trce: MM.PipeBlocks.PipeBlock[0]
      Added block: 'MM.PipeBlocks.Blocks.FuncBlock`2[pbTest.MyContextModel,pbTest.MyValueModel]' to pipe: 'Word Counter'
trce: MM.PipeBlocks.PipeBlock[0]
      Added block: 'pbTest.WordCountBlock' to pipe: 'Word Counter'
trce: MM.PipeBlocks.PipeBlock[0]
      Added block: 'MM.PipeBlocks.Blocks.FuncBlock`2[pbTest.MyContextModel,pbTest.MyValueModel]' to pipe: 'Word Counter'
trce: MM.PipeBlocks.PipeBlock[0]
      Executing pipe: 'Word Counter' synchronously for context: eb5c4f98-a5aa-4e96-8ac5-2f2d9c426d5d
20250425 075300
20250425 075300
trce: MM.PipeBlocks.PipeBlock[0]
      Completed synchronous pipe: 'Word Counter' execution for context: eb5c4f98-a5aa-4e96-8ac5-2f2d9c426d5d
Success: 30475 words
```
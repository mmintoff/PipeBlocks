using Microsoft.Extensions.Options;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Test.BlockBuilderTests;

public class AsyncReturnTests
{
    private readonly BlockBuilder<MyValue> _blockBuilder = new();

    private void AssertIsReturnBlock(IBlock<MyValue> block)
    {
        Assert.IsType<ReturnBlock<MyValue>>(block);
    }

    private void AssertIsBranchBlock(IBlock<MyValue> block)
    {
        Assert.IsType<BranchBlock<MyValue>>(block);
    }

    [Fact]
    public async Task Noop()
    {
        var block = _blockBuilder.Noop();

        Assert.IsType<NoopBlock<MyValue>>(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        var result = await block.ExecuteAsync(value);
        result.Match(
            f => Assert.Fail(f.FailureReason ?? "Empty FailureReason"),
            s => Assert.Equal(value.CorrelationId, result.CorrelationId));
    }

    [Fact]
    public async Task Return_V()
    {
        var block = _blockBuilder.Return();

        AssertIsReturnBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        var result = await block.ExecuteAsync(value);
        result.Match(
            f => Assert.Fail(f.FailureReason ?? "Empty FailureReason"),
            s => Assert.Equal(value.CorrelationId, result.CorrelationId));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task Return_V_Action(bool isFinished, int expected)
    {
        var block = _blockBuilder.Return(v =>
        {
            v.Context.Increment<int>("Counter");
        });

        AssertIsReturnBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFinished
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : new Parameter<MyValue>(initialValue);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "Return_V_Action" }))
            .Then(block)
            ;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFinished = isFinished;
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, result.Context.Get<int>("Counter")),
            s =>
            {
                Assert.Equal(expected, result.Context.Get<int>("Counter"));
                Assert.True(result.Context.IsFinished);
            });
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task Return_V_Func_V(bool isFinished, int expected)
    {
        var block = _blockBuilder.Return(v =>
        {
            v.Context.Increment<int>("Counter");
            return v;
        });

        AssertIsReturnBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFinished
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : new Parameter<MyValue>(initialValue);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "Return_V_Func_V" }))
            .Then(block)
            ;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFinished = isFinished;
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, result.Context.Get<int>("Counter")),
            s =>
            {
                Assert.Equal(expected, result.Context.Get<int>("Counter"));
                Assert.True(result.Context.IsFinished);
            });
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task Return_V_Func_ValueTask(bool isFinished, int expected)
    {
        var block = _blockBuilder.Return(async v =>
        {
            v.Context.Increment<int>("Counter");
            await ValueTask.CompletedTask;
        });

        AssertIsReturnBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFinished
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : new Parameter<MyValue>(initialValue);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "Return_V_Func_ValueTask" }))
            .Then(block)
            ;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFinished = isFinished;
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, result.Context.Get<int>("Counter")),
            s =>
            {
                Assert.Equal(expected, result.Context.Get<int>("Counter"));
                Assert.True(result.Context.IsFinished);
            });
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task Return_V_Func_ValueTask_V(bool isFinished, int expected)
    {
        var block = _blockBuilder.Return(v =>
        {
            v.Context.Increment<int>("Counter");
            return ValueTask.FromResult(v);
        });

        AssertIsReturnBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFinished
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : new Parameter<MyValue>(initialValue);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "Return_V_Func_ValueTask_V" }))
            .Then(block)
            ;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFinished = isFinished;
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, result.Context.Get<int>("Counter")),
            s =>
            {
                Assert.Equal(expected, result.Context.Get<int>("Counter"));
                Assert.True(result.Context.IsFinished);
            });
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 0)] // Will return, therefore will not increment
    [InlineData(false, false, 1)] // Will not return, therefore will increment
    public async Task ReturnIf_V(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(v => condition);

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "ReturnIf_V" }))
            .Then(block)
            .Then(b => b.Run(v => v.Context.Increment<int>("Counter")))
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFinished
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : new Parameter<MyValue>(initialValue);

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFinished = isFinished;
            ctx.Set("Counter", 0);
        });

        result.Match(
            f => Assert.Equal(expected, result.Context.Get<int>("Counter")),
            s => Assert.Equal(expected, result.Context.Get<int>("Counter")));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public async Task ReturnIf_V_Action(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(v => condition, v => v.Context.Increment<int>("Counter"));

        AssertIsBranchBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFinished
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : new Parameter<MyValue>(initialValue);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "ReturnIf_V_Action" }))
            .Then(block)
            ;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFinished = isFinished;
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, result.Context.Get<int>("Counter")),
            s => Assert.Equal(expected, result.Context.Get<int>("Counter")));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public async Task ReturnIf_V_Func_V(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(v => condition, v =>
        {
            v.Context.Increment<int>("Counter");
            return v;
        });

        AssertIsBranchBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFinished
                    ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
                    : new Parameter<MyValue>(initialValue);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "ReturnIf_V_Func_V" }))
            .Then(block)
            ;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFinished = isFinished;
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, result.Context.Get<int>("Counter")),
            s => Assert.Equal(expected, result.Context.Get<int>("Counter")));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public async Task ReturnIf_V_Func_ValueTask(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(v => condition, async v =>
        {
            v.Context.Increment<int>("Counter");
            await ValueTask.CompletedTask;
        });

        AssertIsBranchBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFinished
                    ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
                    : new Parameter<MyValue>(initialValue);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "ReturnIf_V_Func_ValueTask" }))
            .Then(block)
            ;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFinished = isFinished;
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, result.Context.Get<int>("Counter")),
            s => Assert.Equal(expected, result.Context.Get<int>("Counter")));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public async Task ReturnIf_V_Func_ValueTask_V(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(v => condition, v =>
        {
            v.Context.Increment<int>("Counter");
            return ValueTask.FromResult(v);
        });

        AssertIsBranchBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFinished
                            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
                            : new Parameter<MyValue>(initialValue);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "ReturnIf_V_Func_ValueTask_V" }))
            .Then(block)
            ;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFinished = isFinished;
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, result.Context.Get<int>("Counter")),
            s => Assert.Equal(expected, result.Context.Get<int>("Counter")));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public async Task ReturnIf_Async_V_Action(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(v => ValueTask.FromResult(condition), v =>
        {
            v.Context.Increment<int>("Counter");
        });

        AssertIsBranchBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFinished
                            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
                            : new Parameter<MyValue>(initialValue);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "ReturnIf_Async_V_Action" }))
            .Then(block)
            ;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFinished = isFinished;
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, result.Context.Get<int>("Counter")),
            s => Assert.Equal(expected, result.Context.Get<int>("Counter")));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public async Task ReturnIf_Async_V_Func_V(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(v => ValueTask.FromResult(condition), v =>
        {
            v.Context.Increment<int>("Counter");
            return v;
        });

        AssertIsBranchBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFinished
                            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
                            : new Parameter<MyValue>(initialValue);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "ReturnIf_Async_V_Func_V" }))
            .Then(block)
            ;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFinished = isFinished;
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, result.Context.Get<int>("Counter")),
            s => Assert.Equal(expected, result.Context.Get<int>("Counter")));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public async Task ReturnIf_Async_V_Func_ValueTask(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(v => ValueTask.FromResult(condition), async v =>
        {
            v.Context.Increment<int>("Counter");
            await ValueTask.CompletedTask;
        });

        AssertIsBranchBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFinished
                            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
                            : new Parameter<MyValue>(initialValue);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "ReturnIf_Async_V_Func_ValueTask" }))
            .Then(block)
            ;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFinished = isFinished;
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, result.Context.Get<int>("Counter")),
            s => Assert.Equal(expected, result.Context.Get<int>("Counter")));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public async Task ReturnIf_Async_V_Func_ValueTask_V(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(v => ValueTask.FromResult(condition), v =>
        {
            v.Context.Increment<int>("Counter");
            return ValueTask.FromResult(v);
        });

        AssertIsBranchBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFinished
                            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
                            : new Parameter<MyValue>(initialValue);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "ReturnIf_Async_V_Func_ValueTask" }))
            .Then(block)
            ;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFinished = isFinished;
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, result.Context.Get<int>("Counter")),
            s => Assert.Equal(expected, result.Context.Get<int>("Counter")));
    }
}
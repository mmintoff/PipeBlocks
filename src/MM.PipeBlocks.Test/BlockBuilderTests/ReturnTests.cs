using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Test.BlockBuilderTests;

public class ReturnTests
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
    public void Return()
    {
        var block = _blockBuilder.Return();

        AssertIsReturnBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };

        var result = block.Execute(initialValue);
        result.Match(
            f => Assert.Fail(f.FailureReason ?? "Empty FailureReason"),
            s => Assert.Equal(initialValue.Identifier, s.Identifier));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public void Return_Action(bool isFinished, int expected)
    {
        var block = _blockBuilder.Return(v =>
        {
            v.Value.Counter++;
        });

        AssertIsReturnBlock(block);

        var pipe = _blockBuilder.CreatePipe("Return")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };

        var result = pipe.Execute(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s =>
            {
                Assert.Equal(expected, s.Counter);
                Assert.True(result.Context.IsFinished);
            });
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public void Return_Func(bool isFinished, int expected)
    {
        var block = _blockBuilder.Return(v =>
        {
            v.Value.Counter++;
            return v;
        });

        AssertIsReturnBlock(block);

        var pipe = _blockBuilder.CreatePipe("Return")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };

        var result = pipe.Execute(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s =>
            {
                Assert.Equal(expected, s.Counter);
                Assert.True(result.Context.IsFinished);
            });
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public void Return_V_Func_ValueTask(bool isFinished, int expected)
    {
        var block = _blockBuilder.Return(v =>
        {
            v.Value.Counter++;
            return ValueTask.CompletedTask;
        });

        AssertIsReturnBlock(block);

        var pipe = _blockBuilder.CreatePipe("Return")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };

        var result = pipe.Execute(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s =>
            {
                Assert.Equal(expected, s.Counter);
                Assert.True(result.Context.IsFinished);
            });
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public void Return_V_Func_ValueTask_V(bool isFinished, int expected)
    {
        var block = _blockBuilder.Return(v =>
        {
            v.Value.Counter++;
            return ValueTask.FromResult(v);
        });

        AssertIsReturnBlock(block);

        var pipe = _blockBuilder.CreatePipe("Return")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = pipe.Execute(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s =>
            {
                Assert.Equal(expected, s.Counter);
                Assert.True(result.Context.IsFinished);
            });
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 0)] // Will return, therefore will not increment
    [InlineData(false, false, 1)] // Will not return, therefore will increment
    public void ReturnIf_V(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(c => condition);

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("ReturnIf")
            .Then(block)
            .Then(b => b.Run(v => v.Value.Counter++))
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = pipe.Execute(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s => Assert.Equal(expected, s.Counter));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public void ReturnIf_V_Action(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(
            _ => condition,
            v => v.Value.Counter++
            );

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("ReturnIf")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = pipe.Execute(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s => Assert.Equal(expected, s.Counter));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public void ReturnIf_V_Func_V(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(_ => condition, v =>
        {
            v.Value.Counter++;
            return v;
        });

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("ReturnIf")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = pipe.Execute(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s => Assert.Equal(expected, s.Counter));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public void ReturnIf_V_Func_ValueTask(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(_ => condition, v =>
        {
            v.Value.Counter++;
            return ValueTask.CompletedTask;
        });

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("ReturnIf")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = pipe.Execute(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s => Assert.Equal(expected, s.Counter));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public void ReturnIf_V_Func_ValueTask_V(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(_ => condition, v =>
        {
            v.Value.Counter++;
            return ValueTask.FromResult(v);
        });

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("ReturnIf")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = pipe.Execute(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s => Assert.Equal(expected, s.Counter));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public void ReturnIf_Async_V_Action(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(_ => ValueTask.FromResult(condition), v =>
        {
            v.Value.Counter++;
        });

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("ReturnIf")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = pipe.Execute(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s => Assert.Equal(expected, s.Counter));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public void ReturnIf_Async_V_Func_V(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(_ => ValueTask.FromResult(condition), v =>
        {
            v.Value.Counter++;
            return v;
        });

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("ReturnIf")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = pipe.Execute(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s => Assert.Equal(expected, s.Counter));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public void ReturnIf_Async_V_Func_ValueTask(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(_ => ValueTask.FromResult(condition), v =>
        {
            v.Value.Counter++;
            return ValueTask.CompletedTask;
        });

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("ReturnIf")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = pipe.Execute(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s => Assert.Equal(expected, s.Counter));
    }

    [Theory]
    [InlineData(true, true, 0)] // Auto-fail
    [InlineData(true, false, 0)] // Auto-fail
    [InlineData(false, true, 1)] // Will return, therefore will increment
    [InlineData(false, false, 0)] // Will not return, therefore will not increment
    public void ReturnIf_Async_V_Func_ValueTask_V(bool isFinished, bool condition, int expected)
    {
        var block = _blockBuilder.ReturnIf(_c => ValueTask.FromResult(condition), v =>
        {
            v.Value.Counter++;
            return ValueTask.FromResult(v);
        });

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("ReturnIf")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = pipe.Execute(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s => Assert.Equal(expected, s.Counter));
    }
}
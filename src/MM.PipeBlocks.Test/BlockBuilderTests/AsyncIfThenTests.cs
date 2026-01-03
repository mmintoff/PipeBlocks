using Microsoft.Extensions.Options;
using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Test.BlockBuilderTests;
public class AsyncIfThenTests
{
    private readonly BlockBuilder<MyValue> _blockBuilder = new();

    private void AssertIsBranchBlock(IBlock<MyValue> block)
    {
        Assert.IsType<BranchBlock<MyValue>>(block);
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public async Task IfThenElse_NonGeneric(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen(v => v.Context.Get<bool>("Condition"),
                _blockBuilder.ResolveInstance<DoThisBlock>(),
                _blockBuilder.ResolveInstance<ElseThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "IfThenElse" }))
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFlipped = isFlipped;
            ctx.Set("Condition", condition);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null),
            s => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public async Task IfThenElse_NonGeneric_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen(v => ValueTask.FromResult(v.Context.Get<bool>("Condition")),
                _blockBuilder.ResolveInstance<DoThisBlock>(),
                _blockBuilder.ResolveInstance<ElseThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "IfThenElse" }))
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFlipped = isFlipped;
            ctx.Set("Condition", condition);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null),
            s => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null));
    }

    /**/

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public async Task IfThenElse_Generic(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock, ElseThisBlock>(v => v.Context.Get<bool>("Condition"));

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "IfThenElse" }))
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFlipped = isFlipped;
            ctx.Set("Condition", condition);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null),
            s => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public async Task IfThenElse_Generic_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock, ElseThisBlock>(v => ValueTask.FromResult(v.Context.Get<bool>("Condition")));

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "IfThenElse" }))
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFlipped = isFlipped;
            ctx.Set("Condition", condition);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null),
            s => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null));
    }

    /**/

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public async Task IfThen_NonGeneric(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen(v => v.Context.Get<bool>("Condition"),
            _blockBuilder.ResolveInstance<DoThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "IfThenElse" }))
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFlipped = isFlipped;
            ctx.Set("Condition", condition);
        });
        result.Match(
            f => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null),
            s => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public async Task IfThen_NonGeneric_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen(v => ValueTask.FromResult(v.Context.Get<bool>("Condition")),
                _blockBuilder.ResolveInstance<DoThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "IfThenElse" }))
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFlipped = isFlipped;
            ctx.Set("Condition", condition);
        });
        result.Match(
            f => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null),
            s => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null));
    }

    /**/

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public async Task IfThen_Generic(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock>(v => v.Context.Get<bool>("Condition"));

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "IfThenElse" }))
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFlipped = isFlipped;
            ctx.Set("Condition", condition);
        });
        result.Match(
            f => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null),
            s => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public async Task IfThen_Generic_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock>(v => ValueTask.FromResult(v.Context.Get<bool>("Condition")));

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "IfThenElse" }))
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.IsFlipped = isFlipped;
            ctx.Set("Condition", condition);
        });
        result.Match(
            f => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null),
            s => Assert.Equal(resultText, result.Context.TryGet<string>("ResultText", out var value) ? value : null));
    }
}
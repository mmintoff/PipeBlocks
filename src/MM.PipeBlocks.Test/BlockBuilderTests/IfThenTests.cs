using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Test.BlockBuilderTests;

public class IfThenTests
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
    public void IfThenElse_NonGeneric(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen(_ => Context.Get<bool>("Condition"),
                _blockBuilder.ResolveInstance<DoThisBlock>(),
                _blockBuilder.ResolveInstance<ElseThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = pipe.Execute(value, ctx => ctx
            .With("IsFlipped", isFlipped)
            .With("Condition", condition)
            .With("Counter", 0));
        result.Match(
            f => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")),
            s => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public void IfThenElse_NonGeneric_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen(_ => ValueTask.FromResult(Context.Get<bool>("Condition")),
                _blockBuilder.ResolveInstance<DoThisBlock>(),
                _blockBuilder.ResolveInstance<ElseThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = pipe.Execute(value, ctx => ctx
            .With("IsFlipped", isFlipped)
            .With("Condition", condition)
            .With("Counter", 0));
        result.Match(
            f => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")),
            s => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")));
    }

    /**/

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public void IfThenElse_Generic(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock, ElseThisBlock>(_ => Context.Get<bool>("Condition"));

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = pipe.Execute(value, ctx => ctx
            .With("IsFlipped", isFlipped)
            .With("Condition", condition)
            .With("Counter", 0));
        result.Match(
            f => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")),
            s => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public void IfThenElse_Generic_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock, ElseThisBlock>(_ => ValueTask.FromResult(Context.Get<bool>("Condition")));

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = pipe.Execute(value, ctx => ctx
            .With("IsFlipped", isFlipped)
            .With("Condition", condition)
            .With("Counter", 0));
        result.Match(
            f => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")),
            s => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")));

    }

    /**/

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public void IfThen_NonGeneric(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen(_ => Context.Get<bool>("Condition"),
            _blockBuilder.ResolveInstance<DoThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = pipe.Execute(value, ctx => ctx
            .With("IsFlipped", isFlipped)
            .With("Condition", condition)
            .With("Counter", 0));
        result.Match(
            f => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")),
            s => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")));

    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public void IfThen_NonGeneric_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen(_ => ValueTask.FromResult(Context.Get<bool>("Condition")),
                _blockBuilder.ResolveInstance<DoThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = pipe.Execute(value, ctx => ctx
            .With("IsFlipped", isFlipped)
            .With("Condition", condition)
            .With("Counter", 0));
        result.Match(
            f => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")),
            s => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")));
    }

    /**/

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public void IfThen_Generic(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock>(_ => Context.Get<bool>("Condition"));

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = pipe.Execute(value, ctx => ctx
            .With("IsFlipped", isFlipped)
            .With("Condition", condition)
            .With("Counter", 0));
        result.Match(
            f => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")),
            s => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public void IfThen_Generic_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock>(_ => ValueTask.FromResult(Context.Get<bool>("Condition")));

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = isFlipped
            ? new Parameter<MyValue>(new DefaultFailureState<MyValue>(initialValue))
            : initialValue;

        var result = pipe.Execute(value, ctx => ctx
            .With("IsFlipped", isFlipped)
            .With("Condition", condition)
            .With("Counter", 0));
        result.Match(
            f => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")),
            s => Assert.Equal(resultText, result.GetMetaData<string>("ResultText")));

    }
}
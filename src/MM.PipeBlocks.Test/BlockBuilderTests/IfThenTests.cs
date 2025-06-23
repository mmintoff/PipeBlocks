using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Test.BlockBuilderTests;
public class IfThenTests
{
    private readonly BlockBuilder<MyContext, MyValue> _blockBuilder = new();

    private void AssertIsBranchBlock(IBlock<MyContext, MyValue> block)
    {
        Assert.IsType<BranchBlock<MyContext, MyValue>>(block);
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public void IfThenElse_NonGeneric_C(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen(c => c.Condition,
                _blockBuilder.ResolveInstance<DoThisBlock>(),
                _blockBuilder.ResolveInstance<ElseThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public void IfThenElse_NonGeneric_CV(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen((c, v) => c.Condition,
                _blockBuilder.ResolveInstance<DoThisBlock>(),
                _blockBuilder.ResolveInstance<ElseThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public void IfThenElse_NonGeneric_C_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen(c => ValueTask.FromResult(c.Condition),
                _blockBuilder.ResolveInstance<DoThisBlock>(),
                _blockBuilder.ResolveInstance<ElseThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public void IfThenElse_NonGeneric_CV_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen((c, v) => ValueTask.FromResult(c.Condition),
                _blockBuilder.ResolveInstance<DoThisBlock>(),
                _blockBuilder.ResolveInstance<ElseThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    /**/

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public void IfThenElse_Generic_C(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock, ElseThisBlock>(c => c.Condition);

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public void IfThenElse_Generic_CV(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock, ElseThisBlock>((c, v) => c.Condition);

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public void IfThenElse_Generic_C_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock, ElseThisBlock>(c => ValueTask.FromResult(c.Condition));

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(true, false, "ElseThis")]
    [InlineData(false, true, "DoThis")]
    [InlineData(false, false, "ElseThis")]
    public void IfThenElse_Generic_CV_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock, ElseThisBlock>((c, v) => ValueTask.FromResult(c.Condition));

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    /**/

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public void IfThen_NonGeneric_C(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen(c => c.Condition,
            _blockBuilder.ResolveInstance<DoThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public void IfThen_NonGeneric_CV(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen((c, v) => c.Condition,
            _blockBuilder.ResolveInstance<DoThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public void IfThen_NonGeneric_C_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen(c => ValueTask.FromResult(c.Condition),
                _blockBuilder.ResolveInstance<DoThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public void IfThen_NonGeneric_CV_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen((c, v) => ValueTask.FromResult(c.Condition),
                _blockBuilder.ResolveInstance<DoThisBlock>());

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    /**/

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public void IfThen_Generic_C(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock>(c => c.Condition);

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public void IfThen_Generic_CV(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock>((c, v) => c.Condition);

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public void IfThen_Generic_C_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock>(c => ValueTask.FromResult(c.Condition));

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }

    [Theory]
    [InlineData(true, true, "DoThis")]
    [InlineData(false, true, "DoThis")]
    public void IfThen_Generic_CV_ValueTask(bool isFlipped, bool condition, string resultText)
    {
        var block = _blockBuilder.IfThen<DoThisBlock>((c, v) => ValueTask.FromResult(c.Condition));

        AssertIsBranchBlock(block);

        var pipe = _blockBuilder.CreatePipe("IfThenElse")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFlipped
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFlipped = isFlipped,
            Condition = condition
        };

        var result = pipe.Execute(context);
        result.Value.Match(
            f => Assert.Equal(resultText, context.ResultText),
            s => Assert.Equal(resultText, context.ResultText));
    }
}
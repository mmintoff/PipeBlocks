using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Blocks;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace MM.PipeBlocks.Test.BlockBuilderTests;
public class RunTests
{
    private readonly BlockBuilder<MyContext, MyValue> _blockBuilder = new();

    private void AssertIsFuncBlock(IBlock<MyContext, MyValue> block)
    {
        Assert.IsType<FuncBlock<MyContext, MyValue>>(block);
    }

    private void AssertIsAsyncFuncBlock(IBlock<MyContext, MyValue> block)
    {
        Assert.IsType<AsyncFuncBlock<MyContext, MyValue>>(block);
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public void Run_C_Func_C(bool isFinished, int expected)
    {
        var block = _blockBuilder.Run(c =>
        {
            c.Counter++;
            return c;
        });

        AssertIsFuncBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFinished
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFinished = isFinished
        };

        var result = block.Execute(context);
        result.Value.Match(
            f => Assert.Equal(expected, context.Counter),
            s => Assert.Equal(expected, context.Counter));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public void Run_CV_Func_C(bool isFinished, int expected)
    {
        var block = _blockBuilder.Run((c, v) =>
        {
            c.Counter++;
            return c;
        });

        AssertIsFuncBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFinished
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFinished = isFinished
        };

        var result = block.Execute(context);
        result.Value.Match(
            f => Assert.Equal(expected, context.Counter),
            s => Assert.Equal(expected, context.Counter));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public void Run_Action(bool isFinished, int expected)
    {
        int counter = 0;
        var block = _blockBuilder.Run(() =>
        {
            counter++;
        });

        AssertIsFuncBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFinished
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFinished = isFinished
        };

        var result = block.Execute(context);
        result.Value.Match(
            f => Assert.Equal(expected, counter),
            s => Assert.Equal(expected, counter));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public void Run_C_Action(bool isFinished, int expected)
    {
        var block = _blockBuilder.Run(c =>
        {
            c.Counter++;
        });

        AssertIsFuncBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFinished
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFinished = isFinished
        };

        var result = block.Execute(context);
        result.Value.Match(
            f => Assert.Equal(expected, context.Counter),
            s => Assert.Equal(expected, context.Counter));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public void Run_CV_Action(bool isFinished, int expected)
    {
        var block = _blockBuilder.Run((c, v) =>
        {
            c.Counter++;
        });

        AssertIsFuncBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFinished
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFinished = isFinished
        };

        var result = block.Execute(context);
        result.Value.Match(
            f => Assert.Equal(expected, context.Counter),
            s => Assert.Equal(expected, context.Counter));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task Run_AsyncFunc_ValueTask(bool isFinished, int expected)
    {
        int counter = 0;
        var block = _blockBuilder.Run(() =>
        {
            counter++;
            return ValueTask.CompletedTask;
        });

        AssertIsAsyncFuncBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFinished
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFinished = isFinished
        };

        var result = await block.ExecuteAsync(context);
        result.Value.Match(
            f => Assert.Equal(expected, counter),
            s => Assert.Equal(expected, counter));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task Run_AsyncFunc_Task(bool isFinished, int expected)
    {
        int counter = 0;
        var block = _blockBuilder.Run(() =>
        {
            counter++;
            return Task.CompletedTask;
        });

        AssertIsAsyncFuncBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFinished
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFinished = isFinished
        };

        var result = await block.ExecuteAsync(context);
        result.Value.Match(
            f => Assert.Equal(expected, counter),
            s => Assert.Equal(expected, counter));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task Run_C_AsyncFunc_Task(bool isFinished, int expected)
    {
        int counter = 0;
        var block = _blockBuilder.Run(async c =>
        {
            counter++;
            await ValueTask.CompletedTask;
        });

        AssertIsAsyncFuncBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFinished
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFinished = isFinished
        };

        var result = await block.ExecuteAsync(context);
        result.Value.Match(
            f => Assert.Equal(expected, counter),
            s => Assert.Equal(expected, counter));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task Run_CV_AsyncFunc_Task(bool isFinished, int expected)
    {
        int counter = 0;
        var block = _blockBuilder.Run(async (c, v) =>
        {
            counter++;
            await ValueTask.CompletedTask;
        });

        AssertIsAsyncFuncBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFinished
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFinished = isFinished
        };

        var result = await block.ExecuteAsync(context);
        result.Value.Match(
            f => Assert.Equal(expected, counter),
            s => Assert.Equal(expected, counter));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task Run_C_AsyncFunc_ValueTask_C(bool isFinished, int expected)
    {
        var block = _blockBuilder.Run(async c =>
        {
            c.Counter++;
            await Task.FromResult(c);
        });

        AssertIsAsyncFuncBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFinished
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFinished = isFinished
        };

        var result = await block.ExecuteAsync(context);
        result.Value.Match(
            f => Assert.Equal(expected, context.Counter),
            s => Assert.Equal(expected, context.Counter));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task Run_CV_AsyncFunc_ValueTask_C(bool isFinished, int expected)
    {
        var block = _blockBuilder.Run(async (c, v) =>
        {
            c.Counter++;
            await Task.FromResult(c);
        });

        AssertIsAsyncFuncBlock(block);

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(new MyValue())
        {
            Value = isFinished
                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
            IsFinished = isFinished
        };

        var result = await block.ExecuteAsync(context);
        result.Value.Match(
            f => Assert.Equal(expected, context.Counter),
            s => Assert.Equal(expected, context.Counter));
    }
}
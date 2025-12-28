using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Test.BlockBuilderTests;

public class RunTests
{
    private readonly BlockBuilder<MyValue> _blockBuilder = new();

    private static void AssertIsFuncBlock(IBlock<MyValue> block) => Assert.IsType<FuncBlock<MyValue>>(block);

    private static void AssertIsAsyncFuncBlock(IBlock<MyValue> block) => Assert.IsType<AsyncFuncBlock<MyValue>>(block);

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public void Run_V_Func_V(bool isFinished, int expected)
    {
        var block = _blockBuilder.Run(v =>
        {
            v.Value.Counter++;
            return v;
        });

        AssertIsFuncBlock(block);

        var pipe = _blockBuilder.CreatePipe("Run")
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
            f => Assert.Equal(expected, counter),
            s => Assert.Equal(expected, counter));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public void Run_V_Action(bool isFinished, int expected)
    {
        var block = _blockBuilder.Run(v =>
        {
            v.Value.Counter++;
        });

        AssertIsFuncBlock(block);

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

        var pipe = _blockBuilder.CreatePipe("ReturnIf")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = await pipe.ExecuteAsync(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
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

        var pipe = _blockBuilder.CreatePipe("ReturnIf")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = await pipe.ExecuteAsync(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, counter),
            s => Assert.Equal(expected, counter));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task Run_V_AsyncFunc_Task(bool isFinished, int expected)
    {
        int counter = 0;
        var block = _blockBuilder.Run(async c =>
        {
            counter++;
            await ValueTask.CompletedTask;
        });

        AssertIsAsyncFuncBlock(block);

        var pipe = _blockBuilder.CreatePipe("ReturnIf")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = await pipe.ExecuteAsync(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, counter),
            s => Assert.Equal(expected, counter));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task Run_V_AsyncFunc_ValueTask_C(bool isFinished, int expected)
    {
        var block = _blockBuilder.Run(async v =>
        {
            v.Value.Counter++;
            await Task.FromResult(v);
        });

        AssertIsAsyncFuncBlock(block);

        var pipe = _blockBuilder.CreatePipe("ReturnIf")
            .Then(block)
            ;

        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var result = await pipe.ExecuteAsync(initialValue, ctx =>
        {
            ctx.Set("IsFinished", isFinished);
            ctx.Set("Counter", 0);
        });
        result.Match(
            f => Assert.Equal(expected, f.Value.Counter),
            s => Assert.Equal(expected, s.Counter));
    }
}
using Microsoft.Extensions.Options;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Test;

public class SwitchBlockTests
{
    [Fact]
    public void ExecuteSwitch_WithContext()
    {
        var initialValue = new MyValue
        {
            Counter = 0
        };
        bool executedAction = false;
        var value = new Parameter<MyValue>(initialValue);

        var builder = new BlockBuilder<MyValue>();
        var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "test" }))
            .Then(b => b.Switch(v =>
                v.Context.Get<int>("Step") switch
                {
                    0 => b.Run(() => executedAction = true),
                    _ => b.Run(() => executedAction = false),
                }))
            ;

        var result = pipe.Execute(value, ctx =>
        {
            ctx.Set("Step", 0);
        });

        Assert.True(executedAction);
        Assert.Equivalent(value, result);
    }

    [Fact]
    public void ExecuteSwitch_WithValue()
    {
        var initialValue = new MyValue
        {
            Counter = 0
        };
        bool executedAction = false;
        var value = new Parameter<MyValue>(initialValue);

        var builder = new BlockBuilder<MyValue>();
        var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "test" }))
            .Then(b => b.Switch(v =>
                v.Value.Counter switch
                {
                    0 => b.Run(() => executedAction = true),
                    _ => b.Run(() => executedAction = false),
                }))
            ;

        var result = pipe.Execute(value);

        Assert.True(executedAction);
        Assert.Equivalent(value, result);
    }

    [Fact]
    public void ExecuteSwitch_WithFailureState()
    {
        var initialValue = new MyValue
        {
            Counter = 0
        };
        bool executedAction = false;
        var value = new Parameter<MyValue>(initialValue);

        value.SignalBreak(new DefaultFailureState<MyValue>(initialValue)
        {
            FailureReason = "Intentional"
        });

        var builder = new BlockBuilder<MyValue>();
        var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "test" }))
            .Then(b => b.Switch(v =>
                v.Context.Get<int>("Step") switch
                {
                    0 => b.Run(() => executedAction = true),
                    _ => b.Run(() => executedAction = false),
                }))
            ;

        var result = pipe.Execute(value, ctx =>
        {
            ctx.Set("Step", 0);
        });

        Assert.Equivalent(value, result);
        Assert.False(executedAction);

        result.Match(
        x =>
        {
            Assert.Equal(initialValue.Identifier, x.Value.Identifier);
            Assert.Equal(value.CorrelationId, x.CorrelationId);
            Assert.Equal("Intentional", x.FailureReason);
        },
        x => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public void ExecuteAsyncSwitch_WithContext_Sync()
    {
        var initialValue = new MyValue
        {
            Counter = 0
        };
        bool executedAction = false;
        var value = new Parameter<MyValue>(initialValue);

        var builder = new BlockBuilder<MyValue>();
        var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "test" }))
            .Then(b => b.Switch(v =>
            new ValueTask<IBlock<MyValue>>(v.Context.Get<int>("Step") switch
            {
                0 => b.Run(() => executedAction = true),
                _ => b.Run(() => executedAction = false),
            })));

        var result = pipe.Execute(value, ctx =>
        {
            ctx.Set("Step", 0);
        });

        Assert.True(executedAction);
        Assert.Equivalent(value, result);
    }

    ///**/

    [Fact]
    public async Task ExecuteSwitch_WithContext_Async()
    {
        var initialValue = new MyValue
        {
            Counter = 0
        };
        bool executedAction = false;
        var value = new Parameter<MyValue>(initialValue);

        var builder = new BlockBuilder<MyValue>();
        var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "test" }))
            .Then(b => b.Switch(v =>
            new ValueTask<IBlock<MyValue>>(v.Context.Get<int>("Step") switch
            {
                0 => b.Run(() => executedAction = true),
                _ => b.Run(() => executedAction = false),
            })));

        var result = await pipe.ExecuteAsync(value, ctx =>
        {
            ctx.Set("Step", 0);
        });

        Assert.True(executedAction);
        Assert.Equivalent(value, result);
    }

    [Fact]
    public async Task ExecuteSwitch_WithValue_Async()
    {
        var initialValue = new MyValue
        {
            Counter = 0
        };
        bool executedAction = false;
        var value = new Parameter<MyValue>(initialValue);

        var builder = new BlockBuilder<MyValue>();
        var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "test" }))
            .Then(b => b.Switch(v =>
            new ValueTask<IBlock<MyValue>>(v.Value.Counter switch
            {
                0 => b.Run(() => executedAction = true),
                _ => b.Run(() => executedAction = false),
            })));

        var result = await pipe.ExecuteAsync(value);

        Assert.True(executedAction);
        Assert.Equivalent(value, result);
    }

    [Fact]
    public async Task ExecuteSwitch_WithFailureState_Async()
    {
        var initialValue = new MyValue
        {
            Counter = 0
        };
        bool executedAction = false;
        var value = new Parameter<MyValue>(initialValue);

        value.SignalBreak(new DefaultFailureState<MyValue>(initialValue)
        {
            FailureReason = "Intentional"
        });

        var builder = new BlockBuilder<MyValue>();
        var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "test" }))
            .Then(b => b.Switch(v =>
                new ValueTask<IBlock<MyValue>>(v.Value.Counter switch
                {
                    0 => b.Run(() => executedAction = true),
                    _ => b.Run(() => executedAction = false),
                })));

        var result = await pipe.ExecuteAsync(value);

        Assert.Equivalent(value, result);
        Assert.False(executedAction);

        result.Match(
            x =>
            {
                Assert.Equal(initialValue.Identifier, x.Value.Identifier);
                Assert.Equal(value.CorrelationId, x.CorrelationId);
                Assert.Equal("Intentional", x.FailureReason);
            },
            x => Assert.Fail("Expected a failure"));
    }
}
//using MM.PipeBlocks.Abstractions;

//namespace MM.PipeBlocks.Test;
//public class SwitchBlockTests
//{
//    [Fact]
//    public void ExecuteSwitch_WithContext()
//    {
//        var initialValue = new MyValue
//        {
//            Counter = 0
//        };
//        bool executedAction = false;
//        var context = new MyContext(initialValue)
//        {
//            Step = 0
//        };

//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var pipe = builder.CreatePipe("test")
//            .Then(b => b.Switch(c =>
//                c.Step switch
//                {
//                    0 => b.Run(() => executedAction = true),
//                    _ => b.Run(() => executedAction = false),
//                }))
//            ;

//        var result = pipe.Execute(context);

//        Assert.True(executedAction);
//        Assert.Same(context, result);
//    }

//    [Fact]
//    public void ExecuteSwitch_WithValue()
//    {
//        var initialValue = new MyValue
//        {
//            Counter = 0
//        };
//        bool executedAction = false;
//        var context = new MyContext(initialValue);
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var pipe = builder.CreatePipe("test")
//            .Then(b => b.Switch((c, v) =>
//                v.Counter switch
//                {
//                    0 => b.Run(() => executedAction = true),
//                    _ => b.Run(() => executedAction = false),
//                }))
//            ;

//        var result = pipe.Execute(context);

//        Assert.True(executedAction);
//        Assert.Same(context, result);
//    }

//    [Fact]
//    public void ExecuteSwitch_WithFailureState()
//    {
//        var initialValue = new MyValue
//        {
//            Counter = 0
//        };
//        bool executedAction = false;
//        var context = new MyContext(initialValue)
//        {
//            Step = 0
//        };
//        context.SignalBreak(new DefaultFailureState<MyValue>(initialValue)
//        {
//            FailureReason = "Intentional",
//            CorrelationId = context.CorrelationId
//        });

//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var pipe = builder.CreatePipe("test")
//            .Then(b => b.Switch(c =>
//                c.Step switch
//                {
//                    0 => b.Run(() => executedAction = true),
//                    _ => b.Run(() => executedAction = false),
//                }))
//            ;

//        var result = pipe.Execute(context);

//        Assert.Equal(context, result);
//        Assert.False(executedAction);

//        result.Value.Match(
//        x =>
//        {
//            Assert.Equal(initialValue.Identifier, x.Value.Identifier);
//            Assert.Equal(context.CorrelationId, x.CorrelationId);
//            Assert.Equal("Intentional", x.FailureReason);
//        },
//        x => Assert.Fail("Expected a failure"));
//    }

//    [Fact]
//    public void ExecuteAsyncSwitch_WithContext_Sync()
//    {
//        var initialValue = new MyValue
//        {
//            Counter = 0
//        };
//        bool executedAction = false;
//        var context = new MyContext(initialValue)
//        {
//            Step = 0
//        };
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var pipe = builder.CreatePipe("test")
//            .Then(b => b.Switch(c =>
//            new ValueTask<IBlock<MyContext, MyValue>>(c.Step switch
//            {
//                0 => b.Run(() => executedAction = true),
//                _ => b.Run(() => executedAction = false),
//            })));

//        var result = pipe.Execute(context);

//        Assert.True(executedAction);
//        Assert.Same(context, result);
//    }

//    /**/

//    [Fact]
//    public async Task ExecuteSwitch_WithContext_Async()
//    {
//        var initialValue = new MyValue
//        {
//            Counter = 0
//        };
//        bool executedAction = false;
//        var context = new MyContext(initialValue)
//        {
//            Step = 0
//        };
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var pipe = builder.CreatePipe("test")
//            .Then(b => b.Switch(c =>
//            new ValueTask<IBlock<MyContext, MyValue>>(c.Step switch
//            {
//                0 => b.Run(() => executedAction = true),
//                _ => b.Run(() => executedAction = false),
//            })));

//        var result = await pipe.ExecuteAsync(context);

//        Assert.True(executedAction);
//        Assert.Same(context, result);
//    }

//    [Fact]
//    public async Task ExecuteSwitch_WithValue_Async()
//    {
//        var initialValue = new MyValue
//        {
//            Counter = 0
//        };
//        bool executedAction = false;
//        var context = new MyContext(initialValue);

//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var pipe = builder.CreatePipe("test")
//            .Then(b => b.Switch((c, v) =>
//            new ValueTask<IBlock<MyContext, MyValue>>(v.Counter switch
//            {
//                0 => b.Run(() => executedAction = true),
//                _ => b.Run(() => executedAction = false),
//            })));

//        var result = await pipe.ExecuteAsync(context);

//        Assert.True(executedAction);
//        Assert.Same(context, result);
//    }

//    [Fact]
//    public async Task ExecuteSwitch_WithFailureState_Async()
//    {
//        var initialValue = new MyValue
//        {
//            Counter = 0
//        };
//        bool executedAction = false;
//        var context = new MyContext(initialValue)
//        {
//            Step = 0
//        };
//        context.SignalBreak(new DefaultFailureState<MyValue>(initialValue)
//        {
//            FailureReason = "Intentional",
//            CorrelationId = context.CorrelationId
//        });

//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var pipe = builder.CreatePipe("test")
//            .Then(b => b.Switch(c =>
//                new ValueTask<IBlock<MyContext, MyValue>>(c.Step switch
//                {
//                    0 => b.Run(() => executedAction = true),
//                    _ => b.Run(() => executedAction = false),
//                })));

//        var result = await pipe.ExecuteAsync(context);

//        Assert.Equal(context, result);
//        Assert.False(executedAction);

//        result.Value.Match(
//        x =>
//        {
//            Assert.Equal(initialValue.Identifier, x.Value.Identifier);
//            Assert.Equal(context.CorrelationId, x.CorrelationId);
//            Assert.Equal("Intentional", x.FailureReason);
//        },
//        x => Assert.Fail("Expected a failure"));
//    }

//    [Fact]
//    public async Task ExecuteSyncSwitch_WithContext_Async()
//    {
//        var initialValue = new MyValue
//        {
//            Counter = 0
//        };
//        bool executedAction = false;
//        var context = new MyContext(initialValue)
//        {
//            Step = 0
//        };

//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var pipe = builder.CreatePipe("test")
//            .Then(b => b.Switch(c =>
//                c.Step switch
//                {
//                    0 => b.Run(() => executedAction = true),
//                    _ => b.Run(() => executedAction = false),
//                }))
//            ;

//        var result = await pipe.ExecuteAsync(context);

//        Assert.True(executedAction);
//        Assert.Same(context, result);
//    }
//}
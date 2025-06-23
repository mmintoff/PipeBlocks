namespace MM.PipeBlocks.Test;
public class FuncBlockTests
{
    [Fact]
    public void Execute_WithSimpleAction_ExecutesAction()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        bool actionExecuted = false;
        void simpleAction(MyContext c) => actionExecuted = true;
        var actionBlock = new FuncBlock<MyContext, MyValue>(simpleAction);

        // Act
        var result = actionBlock.Execute(context);

        // Assert
        Assert.True(actionExecuted);
        Assert.Same(context, result);
        Assert.Equal("MM.PipeBlocks.Blocks.FuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass0_0.<Execute_WithSimpleAction_ExecutesAction>g__simpleAction|0)", actionBlock.ToString());
    }

    [Fact]
    public void Execute_WithValueAction_ExecutesAction()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        bool actionExecuted = false;
        void valueAction(MyContext c, MyValue v) => actionExecuted = true;
        var actionBlock = new FuncBlock<MyContext, MyValue>(valueAction);

        // Act
        var result = actionBlock.Execute(context);

        // Assert
        Assert.True(actionExecuted);
        Assert.Same(context, result);
        Assert.Equal("MM.PipeBlocks.Blocks.FuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass1_0.<Execute_WithValueAction_ExecutesAction>g__valueAction|0)", actionBlock.ToString());
    }

    [Fact]
    public void Execute_WithValueAction_ReturnsFailureContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        bool actionExecuted = false;
        void valueAction(MyContext c, MyValue v)
        {
            c.Value.Match(
                _ => { },
                _ => c.SignalBreak(new DefaultFailureState<MyValue>(initialValue)
                {
                    FailureReason = "Intentional",
                    CorrelationId = c.CorrelationId
                }));
            actionExecuted = true;
        }
        var actionBlock = new FuncBlock<MyContext, MyValue>(valueAction);

        // Act
        var result = actionBlock.Execute(context);

        // Assert
        result.Value.Match(
            f =>
            {
                Assert.Equal(initialValue.Identifier, f.Value.Identifier);
                Assert.Equal(context.CorrelationId, f.CorrelationId);
                Assert.Equal("Intentional", f.FailureReason);
                Assert.True(actionExecuted);
                Assert.Equal("MM.PipeBlocks.Blocks.FuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass2_0.<Execute_WithValueAction_ReturnsFailureContext>g__valueAction|0)", actionBlock.ToString());
            },
            _ => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public void Execute_WithFailContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        context.SignalBreak(new DefaultFailureState<MyValue>(initialValue)
        {
            FailureReason = "Initial Failure",
            CorrelationId = context.CorrelationId
        });
        bool actionExecuted = false;
        void valueAction(MyContext c, MyValue v) => actionExecuted = true;
        var actionBlock = new FuncBlock<MyContext, MyValue>(valueAction);

        // Act
        var result = actionBlock.Execute(context);

        // Assert
        result.Value.Match(
            f =>
            {
                Assert.Equal(initialValue.Identifier, f.Value.Identifier);
                Assert.Equal(context.CorrelationId, f.CorrelationId);
                Assert.Equal("Initial Failure", f.FailureReason);
                Assert.False(actionExecuted);
                Assert.Equal("MM.PipeBlocks.Blocks.FuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass3_0.<Execute_WithFailContext>g__valueAction|0)", actionBlock.ToString());
            },
            _ => Assert.Fail("Expected a failure"));
    }

    /**/

    [Fact]
    public async Task Execute_WithSimpleAction_ExecutesAction_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        bool actionExecuted = false;
        ValueTask simpleAction(MyContext c)
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        }
        var actionBlock = new AsyncFuncBlock<MyContext, MyValue>(simpleAction);

        // Act
        var result = await actionBlock.ExecuteAsync(context);

        // Assert
        Assert.True(actionExecuted);
        Assert.Same(context, result);
        Assert.Equal("MM.PipeBlocks.Blocks.AsyncFuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass4_0.<Execute_WithSimpleAction_ExecutesAction_Async>g__simpleAction|0)", actionBlock.ToString());
    }

    [Fact]
    public async Task Execute_WithValueAction_ExecutesAction_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        bool actionExecuted = false;
        ValueTask valueAction(MyContext c, MyValue v)
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        }
        var actionBlock = new AsyncFuncBlock<MyContext, MyValue>(valueAction);

        // Act
        var result = await actionBlock.ExecuteAsync(context);

        // Assert
        Assert.True(actionExecuted);
        Assert.Same(context, result);
        Assert.Equal("MM.PipeBlocks.Blocks.AsyncFuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass5_0.<Execute_WithValueAction_ExecutesAction_Async>g__valueAction|0)", actionBlock.ToString());
    }

    [Fact]
    public async Task Execute_WithValueAction_ReturnsFailureContext_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        bool actionExecuted = false;
        ValueTask valueAction(MyContext c, MyValue v)
        {
            c.Value.Match(
                _ => { },
                s => c.SignalBreak(new DefaultFailureState<MyValue>(s)
                {
                    FailureReason = "Intentional",
                    CorrelationId = c.CorrelationId
                }));
            actionExecuted = true;
            return ValueTask.CompletedTask;
        }
        var actionBlock = new AsyncFuncBlock<MyContext, MyValue>(valueAction);

        // Act
        var result = await actionBlock.ExecuteAsync(context);

        // Assert
        result.Value.Match(
            f =>
            {
                Assert.Equal(initialValue.Identifier, f.Value.Identifier);
                Assert.Equal(context.CorrelationId, f.CorrelationId);
                Assert.Equal("Intentional", f.FailureReason);
                Assert.True(actionExecuted);
                Assert.Equal("MM.PipeBlocks.Blocks.AsyncFuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass6_0.<Execute_WithValueAction_ReturnsFailureContext_Async>g__valueAction|0)", actionBlock.ToString());
            },
            _ => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public async Task Execute_WithFailContext_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        context.SignalBreak(new DefaultFailureState<MyValue>(initialValue)
        {
            FailureReason = "Initial Failure",
            CorrelationId = context.CorrelationId
        });
        bool actionExecuted = false;
        ValueTask valueAction(MyContext c, MyValue v)
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        }
        var actionBlock = new AsyncFuncBlock<MyContext, MyValue>(valueAction);

        // Act
        var result = await actionBlock.ExecuteAsync(context);

        // Assert
        result.Value.Match(
            x =>
            {
                Assert.Equal(initialValue.Identifier, x.Value.Identifier);
                Assert.Equal(context.CorrelationId, x.CorrelationId);
                Assert.Equal("Initial Failure", x.FailureReason);
                Assert.False(actionExecuted);
                Assert.Equal("MM.PipeBlocks.Blocks.AsyncFuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass7_0.<Execute_WithFailContext_Async>g__valueAction|0)", actionBlock.ToString());
            },
            x => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public void Execute_WithSimpleFunction_ReturnsModifiedContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        static MyContext simpleFunc(MyContext c) => new(new MyValue());
        var funcBlock = new FuncBlock<MyContext, MyValue>(simpleFunc);

        // Act
        var result = funcBlock.Execute(context);

        // Assert
        Assert.NotSame(context, result);
        Assert.Equal("MM.PipeBlocks.Blocks.FuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithSimpleFunction_ReturnsModifiedContext>g__simpleFunc|8_0)", funcBlock.ToString());
    }

    [Fact]
    public void Execute_WithSimpleFunction_ReturnsFailureContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        MyContext simpleFunc(MyContext c)
        {
            c.Value.Match(
                _ => { },
                s => c.SignalBreak(new DefaultFailureState<MyValue>(s)
                {
                    FailureReason = "Intentional",
                    CorrelationId = c.CorrelationId
                }));
            return c;
        }
        var funcBlock = new FuncBlock<MyContext, MyValue>(simpleFunc);

        // Act
        var result = funcBlock.Execute(context);

        // Assert
        result.Value.Match(
            x =>
            {
                Assert.Equal(initialValue.Identifier, x.Value.Identifier);
                Assert.Equal(context.CorrelationId, x.CorrelationId);
                Assert.Equal("Intentional", x.FailureReason);
                Assert.Equal("MM.PipeBlocks.Blocks.FuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithSimpleFunction_ReturnsFailureContext>g__simpleFunc|9_0)", funcBlock.ToString());
            },
            x => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public void Execute_WithValueFunction_ReturnsModifiedContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        static MyContext valueFunc(MyContext c, MyValue v) => new(new MyValue());
        var funcBlock = new FuncBlock<MyContext, MyValue>(valueFunc);

        // Act
        var result = funcBlock.Execute(context);

        // Assert
        Assert.NotSame(context, result);
        Assert.Equal("MM.PipeBlocks.Blocks.FuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithValueFunction_ReturnsModifiedContext>g__valueFunc|10_0)", funcBlock.ToString());
    }

    [Fact]
    public void Execute_WithValueFunction_ReturnsFailedContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        MyContext valueFunc(MyContext c, MyValue v)
        {
            c.Value.Match(
                _ => { },
                s => c.SignalBreak(new DefaultFailureState<MyValue>(s)
                {
                    FailureReason = "Intentional",
                    CorrelationId = c.CorrelationId
                }));
            return c;
        }
        var funcBlock = new FuncBlock<MyContext, MyValue>(valueFunc);

        // Act
        var result = funcBlock.Execute(context);

        // Assert
        result.Value.Match(
            x =>
            {
                Assert.Equal(initialValue.Identifier, x.Value.Identifier);
                Assert.Equal(context.CorrelationId, x.CorrelationId);
                Assert.Equal("Intentional", x.FailureReason);
                Assert.Equal("MM.PipeBlocks.Blocks.FuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithValueFunction_ReturnsFailedContext>g__valueFunc|11_0)", funcBlock.ToString());
            },
            x => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public void Execute_WithSimpleFunction_ReturnsSameContext()
    {
        // Arrange
        var context = new MyContext(new MyValue());
        static MyContext simpleFunc(MyContext c) => c;
        var funcBlock = new FuncBlock<MyContext, MyValue>(simpleFunc);

        // Act
        var result = funcBlock.Execute(context);

        // Assert
        Assert.Same(context, result);
        Assert.Equal("MM.PipeBlocks.Blocks.FuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithSimpleFunction_ReturnsSameContext>g__simpleFunc|12_0)", funcBlock.ToString());
    }

    [Fact]
    public void Execute_WithValueFunction_ReturnsSameContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        static MyContext valueFunc(MyContext c, MyValue v) => c;
        var funcBlock = new FuncBlock<MyContext, MyValue>(valueFunc);

        // Act
        var result = funcBlock.Execute(context);

        // Assert
        Assert.Same(context, result);
        Assert.Equal("MM.PipeBlocks.Blocks.FuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithValueFunction_ReturnsSameContext>g__valueFunc|13_0)", funcBlock.ToString());
    }

    /**/

    [Fact]
    public async Task Execute_WithSimpleFunction_ReturnsModifiedContext_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        static ValueTask<MyContext> simpleFunc(MyContext c) => ValueTask.FromResult(new MyContext(new MyValue()));
        var funcBlock = new AsyncFuncBlock<MyContext, MyValue>(simpleFunc);

        // Act
        var result = await funcBlock.ExecuteAsync(context);

        // Assert
        Assert.NotSame(context, result);
        Assert.Equal("MM.PipeBlocks.Blocks.AsyncFuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithSimpleFunction_ReturnsModifiedContext_Async>g__simpleFunc|14_0)", funcBlock.ToString());
    }

    [Fact]
    public async Task Execute_WithSimpleFunction_ReturnsFailureContext_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        ValueTask<MyContext> simpleFunc(MyContext c)
        {
            c.Value.Match(
                _ => { },
                s => c.SignalBreak(new DefaultFailureState<MyValue>(s)
                {
                    FailureReason = "Intentional",
                    CorrelationId = c.CorrelationId
                }));
            return ValueTask.FromResult(c);
        }
        var funcBlock = new AsyncFuncBlock<MyContext, MyValue>(simpleFunc);

        // Act
        var result = await funcBlock.ExecuteAsync(context);

        // Assert
        result.Value.Match(
            x =>
            {
                Assert.Equal(initialValue.Identifier, x.Value.Identifier);
                Assert.Equal(context.CorrelationId, x.CorrelationId);
                Assert.Equal("Intentional", x.FailureReason);
                Assert.Equal("MM.PipeBlocks.Blocks.AsyncFuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithSimpleFunction_ReturnsFailureContext_Async>g__simpleFunc|15_0)", funcBlock.ToString());
            },
            x => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public async Task Execute_WithValueFunction_ReturnsModifiedContext_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        static ValueTask<MyContext> valueFunc(MyContext c, MyValue v) => ValueTask.FromResult(new MyContext(new MyValue()));
        var funcBlock = new AsyncFuncBlock<MyContext, MyValue>(valueFunc);

        // Act
        var result = await funcBlock.ExecuteAsync(context);

        // Assert
        Assert.NotSame(context, result);
        Assert.Equal("MM.PipeBlocks.Blocks.AsyncFuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithValueFunction_ReturnsModifiedContext_Async>g__valueFunc|16_0)", funcBlock.ToString());
    }

    [Fact]
    public async Task Execute_WithValueFunction_ReturnsFailedContext_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        ValueTask<MyContext> valueFunc(MyContext c, MyValue v)
        {
            c.Value.Match(
                _ => { },
                s => c.SignalBreak(new DefaultFailureState<MyValue>(s)
                {
                    FailureReason = "Intentional",
                    CorrelationId = c.CorrelationId
                }));
            return ValueTask.FromResult(c);
        }
        var funcBlock = new AsyncFuncBlock<MyContext, MyValue>(valueFunc);

        // Act
        var result = await funcBlock.ExecuteAsync(context);

        // Assert
        result.Value.Match(
            x =>
            {
                Assert.Equal(initialValue.Identifier, x.Value.Identifier);
                Assert.Equal(context.CorrelationId, x.CorrelationId);
                Assert.Equal("Intentional", x.FailureReason);
                Assert.Equal("MM.PipeBlocks.Blocks.AsyncFuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithValueFunction_ReturnsFailedContext_Async>g__valueFunc|17_0)", funcBlock.ToString());
            },
            x => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public async Task Execute_WithSimpleFunction_ReturnsSameContext_Async()
    {
        // Arrange
        var context = new MyContext(new MyValue());
        static ValueTask<MyContext> simpleFunc(MyContext c) => ValueTask.FromResult(c);
        var funcBlock = new AsyncFuncBlock<MyContext, MyValue>(simpleFunc);

        // Act
        var result = await funcBlock.ExecuteAsync(context);

        // Assert
        Assert.Same(context, result);
        Assert.Equal("MM.PipeBlocks.Blocks.AsyncFuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithSimpleFunction_ReturnsSameContext_Async>g__simpleFunc|18_0)", funcBlock.ToString());
    }

    [Fact]
    public async Task Execute_WithValueFunction_ReturnsSameContext_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var context = new MyContext(initialValue);
        static ValueTask<MyContext> valueFunc(MyContext c, MyValue v) => ValueTask.FromResult(c);
        var funcBlock = new AsyncFuncBlock<MyContext, MyValue>(valueFunc);

        // Act
        var result = await funcBlock.ExecuteAsync(context);

        // Assert
        Assert.Same(context, result);
        Assert.Equal("MM.PipeBlocks.Blocks.AsyncFuncBlock`2[MM.PipeBlocks.Test.MyContext,MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithValueFunction_ReturnsSameContext_Async>g__valueFunc|19_0)", funcBlock.ToString());
    }
}
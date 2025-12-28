using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Test;

public class FuncBlockTests
{
    [Fact]
    public void Execute_WithSimpleAction_ExecutesAction()
    {
        // Arrange
        var initialValue = new MyValue();
        var value = new Parameter<MyValue>(initialValue);

        bool actionExecuted = false;
        void simpleAction(Parameter<MyValue> V) => actionExecuted = true;
        var actionBlock = new FuncBlock<MyValue>(simpleAction);

        // Act
        var result = actionBlock.Execute(value);

        // Assert
        Assert.True(actionExecuted);
        Assert.Equivalent(value, result);
        Assert.Equal("MM.PipeBlocks.FuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass0_0.<Execute_WithSimpleAction_ExecutesAction>g__simpleAction|0)", actionBlock.ToString());
    }

    [Fact]
    public void Execute_WithValueAction_ReturnsFailureContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var value = new Parameter<MyValue>(initialValue);

        bool actionExecuted = false;
        void valueAction(Parameter<MyValue> v)
        {
            v.Match(
                _ => { },
                _ => v.SignalBreak(new DefaultFailureState<MyValue>(initialValue)
                {
                    FailureReason = "Intentional",
                    CorrelationId = v.CorrelationId
                }));
            actionExecuted = true;
        }
        var actionBlock = new FuncBlock<MyValue>(valueAction);

        // Act
        var result = actionBlock.Execute(value);

        // Assert
        result.Match(
            f =>
            {
                Assert.Equal(initialValue.Identifier, f.Value.Identifier);
                Assert.Equal(value.CorrelationId, f.CorrelationId);
                Assert.Equal("Intentional", f.FailureReason);
                Assert.True(actionExecuted);
                Assert.Equal("MM.PipeBlocks.FuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass1_0.<Execute_WithValueAction_ReturnsFailureContext>g__valueAction|0)", actionBlock.ToString());
            },
            _ => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public void Execute_WithFailContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var value = new Parameter<MyValue>(initialValue);

        value.SignalBreak(new DefaultFailureState<MyValue>(initialValue)
        {
            FailureReason = "Initial Failure"
        });
        bool actionExecuted = false;
        void valueAction(Parameter<MyValue> v) => actionExecuted = true;
        var actionBlock = new FuncBlock<MyValue>(valueAction);

        // Act
        var result = actionBlock.Execute(value);

        // Assert
        result.Match(
            f =>
            {
                Assert.Equal(initialValue.Identifier, f.Value.Identifier);
                Assert.Equal(value.CorrelationId, f.CorrelationId);
                Assert.Equal("Initial Failure", f.FailureReason);
                Assert.False(actionExecuted);
                Assert.Equal("MM.PipeBlocks.FuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass2_0.<Execute_WithFailContext>g__valueAction|0)", actionBlock.ToString());
            },
            _ => Assert.Fail("Expected a failure"));
    }

    /**/

    [Fact]
    public async Task Execute_WithSimpleAction_ExecutesAction_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var value = new Parameter<MyValue>(initialValue);

        bool actionExecuted = false;
        ValueTask simpleAction(Parameter<MyValue> p)
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        }
        var actionBlock = new AsyncFuncBlock<MyValue>(simpleAction);

        // Act
        var result = await actionBlock.ExecuteAsync(value);

        // Assert
        Assert.True(actionExecuted);
        Assert.Equivalent(value, result);
        Assert.Equal("MM.PipeBlocks.AsyncFuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass3_0.<Execute_WithSimpleAction_ExecutesAction_Async>g__simpleAction|0)", actionBlock.ToString());
    }

    [Fact]
    public async Task Execute_WithValueAction_ExecutesAction_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var value = new Parameter<MyValue>(initialValue);

        bool actionExecuted = false;
        ValueTask valueAction(Parameter<MyValue> p)
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        }
        var actionBlock = new AsyncFuncBlock<MyValue>(valueAction);

        // Act
        var result = await actionBlock.ExecuteAsync(value);

        // Assert
        Assert.True(actionExecuted);
        Assert.Equivalent(value, result);
        Assert.Equal("MM.PipeBlocks.AsyncFuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass4_0.<Execute_WithValueAction_ExecutesAction_Async>g__valueAction|0)", actionBlock.ToString());
    }

    [Fact]
    public async Task Execute_WithValueAction_ReturnsFailureContext_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var value = new Parameter<MyValue>(initialValue);

        bool actionExecuted = false;
        ValueTask valueAction(Parameter<MyValue> p)
        {
            p.Match(
                _ => { },
                s => p.SignalBreak(new DefaultFailureState<MyValue>(s)
                {
                    FailureReason = "Intentional",
                    CorrelationId = p.CorrelationId
                }));
            actionExecuted = true;
            return ValueTask.CompletedTask;
        }
        var actionBlock = new AsyncFuncBlock<MyValue>(valueAction);

        // Act
        var result = await actionBlock.ExecuteAsync(value);

        // Assert
        result.Match(
            f =>
            {
                Assert.Equal(initialValue.Identifier, f.Value.Identifier);
                Assert.Equal(result.CorrelationId, f.CorrelationId);
                Assert.Equal("Intentional", f.FailureReason);
                Assert.True(actionExecuted);
                Assert.Equal("MM.PipeBlocks.AsyncFuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass5_0.<Execute_WithValueAction_ReturnsFailureContext_Async>g__valueAction|0)", actionBlock.ToString());
            },
            _ => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public async Task Execute_WithFailContext_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var value = new Parameter<MyValue>(initialValue);

        value.SignalBreak(new DefaultFailureState<MyValue>(initialValue)
        {
            FailureReason = "Initial Failure"
        });
        bool actionExecuted = false;
        ValueTask valueAction(Parameter<MyValue> v)
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        }
        var actionBlock = new AsyncFuncBlock<MyValue>(valueAction);

        // Act
        var result = await actionBlock.ExecuteAsync(value);

        // Assert
        result.Match(
            x =>
            {
                Assert.Equal(initialValue.Identifier, x.Value.Identifier);
                Assert.Equal(result.CorrelationId, x.CorrelationId);
                Assert.Equal("Initial Failure", x.FailureReason);
                Assert.False(actionExecuted);
                Assert.Equal("MM.PipeBlocks.AsyncFuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests+<>c__DisplayClass6_0.<Execute_WithFailContext_Async>g__valueAction|0)", actionBlock.ToString());
            },
            x => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public void Execute_WithSimpleFunction_ReturnsModifiedContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var value = new Parameter<MyValue>(initialValue);

        static Parameter<MyValue> simpleFunc(Parameter<MyValue> v) => new(new MyValue());
        var funcBlock = new FuncBlock<MyValue>(simpleFunc);

        // Act
        var result = funcBlock.Execute(value);

        // Assert
        Assert.NotSame(value, result);
        Assert.Equal("MM.PipeBlocks.FuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithSimpleFunction_ReturnsModifiedContext>g__simpleFunc|7_0)", funcBlock.ToString());
    }

    [Fact]
    public void Execute_WithSimpleFunction_ReturnsFailureContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var value = new Parameter<MyValue>(initialValue);

        Parameter<MyValue> simpleFunc(Parameter<MyValue> p)
        {
            p.Match(
                _ => { },
                s => p.SignalBreak(new DefaultFailureState<MyValue>(s)
                {
                    FailureReason = "Intentional"
                }));
            return p;
        }
        var funcBlock = new FuncBlock<MyValue>(simpleFunc);

        // Act
        var result = funcBlock.Execute(value);

        // Assert
        result.Match(
            x =>
            {
                Assert.Equal(initialValue.Identifier, x.Value.Identifier);
                Assert.Equal(value.CorrelationId, x.CorrelationId);
                Assert.Equal("Intentional", x.FailureReason);
                Assert.Equal("MM.PipeBlocks.FuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithSimpleFunction_ReturnsFailureContext>g__simpleFunc|8_0)", funcBlock.ToString());
            },
            x => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public void Execute_WithValueFunction_ReturnsModifiedContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var value = new Parameter<MyValue>(initialValue);

        static Parameter<MyValue> valueFunc(Parameter<MyValue> v) => new(new MyValue());
        var funcBlock = new FuncBlock<MyValue>(valueFunc);

        // Act
        var result = funcBlock.Execute(value);

        // Assert
        Assert.NotSame(value, result);
        Assert.Equal("MM.PipeBlocks.FuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithValueFunction_ReturnsModifiedContext>g__valueFunc|9_0)", funcBlock.ToString());
    }

    [Fact]
    public void Execute_WithValueFunction_ReturnsFailedContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var value = new Parameter<MyValue>(initialValue);

        Parameter<MyValue> valueFunc(Parameter<MyValue> v)
        {
            v.Match(
                _ => { },
                s => v.SignalBreak(new DefaultFailureState<MyValue>(s)
                {
                    FailureReason = "Intentional"
                }));
            return v;
        }
        var funcBlock = new FuncBlock<MyValue>(valueFunc);

        // Act
        var result = funcBlock.Execute(value);

        // Assert
        result.Match(
            x =>
            {
                Assert.Equal(initialValue.Identifier, x.Value.Identifier);
                Assert.Equal(value.CorrelationId, x.CorrelationId);
                Assert.Equal("Intentional", x.FailureReason);
                Assert.Equal("MM.PipeBlocks.FuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithValueFunction_ReturnsFailedContext>g__valueFunc|10_0)", funcBlock.ToString());
            },
            x => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public void Execute_WithSimpleFunction_ReturnsSameContext()
    {
        // Arrange
        var initialValue = new MyValue();
        var value = new Parameter<MyValue>(initialValue);

        static Parameter<MyValue> simpleFunc(Parameter<MyValue> v) => v;
        var funcBlock = new FuncBlock<MyValue>(simpleFunc);

        // Act
        var result = funcBlock.Execute(value);

        // Assert
        Assert.Equivalent(value, result);
        Assert.Equal("MM.PipeBlocks.FuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithSimpleFunction_ReturnsSameContext>g__simpleFunc|11_0)", funcBlock.ToString());
    }

    ///**/

    [Fact]
    public async Task Execute_WithSimpleFunction_ReturnsModifiedContext_Async()
    {
        // Arrange
        var initialValue = new MyValue() { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        static ValueTask<Parameter<MyValue>> simpleFunc(Parameter<MyValue> p) => ValueTask.FromResult(new Parameter<MyValue>(new MyValue { Identifier = Guid.NewGuid() }));
        var funcBlock = new AsyncFuncBlock<MyValue>(simpleFunc);

        // Act
        var result = await funcBlock.ExecuteAsync(value);

        // Assert
        Assert.NotSame(value, result);
        Assert.Equal("MM.PipeBlocks.AsyncFuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithSimpleFunction_ReturnsModifiedContext_Async>g__simpleFunc|12_0)", funcBlock.ToString());
    }

    [Fact]
    public async Task Execute_WithSimpleFunction_ReturnsFailureContext_Async()
    {
        // Arrange
        var initialValue = new MyValue();
        var value = new Parameter<MyValue>(initialValue);

        ValueTask<Parameter<MyValue>> simpleFunc(Parameter<MyValue> p)
        {
            p.Match(
                _ => { },
                s => p.SignalBreak(new DefaultFailureState<MyValue>(s)
                {
                    FailureReason = "Intentional"
                }));
            return ValueTask.FromResult(p);
        }
        var funcBlock = new AsyncFuncBlock<MyValue>(simpleFunc);

        // Act
        var result = await funcBlock.ExecuteAsync(value);

        // Assert
        result.Match(
            x =>
            {
                Assert.Equal(initialValue.Identifier, x.Value.Identifier);
                Assert.Equal(result.CorrelationId, x.CorrelationId);
                Assert.Equal("Intentional", x.FailureReason);
                Assert.Equal("MM.PipeBlocks.AsyncFuncBlock`1[MM.PipeBlocks.Test.MyValue] (Method: MM.PipeBlocks.Test.FuncBlockTests.<Execute_WithSimpleFunction_ReturnsFailureContext_Async>g__simpleFunc|13_0)", funcBlock.ToString());
            },
            x => Assert.Fail("Expected a failure"));
    }
}
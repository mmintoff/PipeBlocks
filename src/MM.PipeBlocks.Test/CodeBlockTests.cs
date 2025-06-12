namespace MM.PipeBlocks.Test;
public class CodeBlockTests
{
    [Fact]
    public void Execute_WithFailContext()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(initialValue);
        context.SignalBreak(new DefaultFailureState<MyValue>(initialValue)
        {
            FailureReason = "Initial Failure",
            CorrelationId = context.CorrelationId
        });

        var block = new ReturnContext_CodeBlock();
        var result = block.Execute(context);

        Assert.Equal(context.CorrelationId, result.CorrelationId);
        result.Value.Match(
            f =>
            {
                Assert.Equal(initialValue.Identifier, f.Value.Identifier);
                Assert.Equal(context.CorrelationId, f.CorrelationId);
                Assert.Equal("Initial Failure", f.FailureReason);
            },
            s => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public void Execute_State()
    {
        var value = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(value);

        var block = new ReturnContext_CodeBlock();

        var result = block.Execute(context);

        Assert.Equal(context.CorrelationId, result.CorrelationId);

        result.Value.Match(
            f => Assert.Fail(f.FailureReason ?? "Empty FailureReason"),
            s => Assert.Equal(value.Identifier, s.Identifier));
    }
    
    [Fact]
    public void Execute_Fail_State()
    {
        var value = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(value);

        var block = new ReturnFailContext_CodeBlock();

        var result = block.Execute(context);

        Assert.Equal(context.CorrelationId, result.CorrelationId);

        result.Value.Match(
            f =>
            {
                Assert.Equal(value.Identifier, f.Value.Identifier);
                Assert.Equal(context.CorrelationId, f.CorrelationId);
                Assert.Equal("Intentional", f.FailureReason);
            },
            _ => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public void Execute_Exception_State()
    {
        var value = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(value);

        var block = new Exception_CodeBlock();

        try
        {
            var result = block.Execute(context);
            Assert.Fail("Expected exception");
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }
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
        var block = new ReturnContext_AsyncCodeBlock();

        // Act
        var result = await block.ExecuteAsync(context);

        Assert.Equal(context.CorrelationId, result.CorrelationId);

        // Assert
        result.Value.Match(
            x =>
            {
                Assert.Equal(initialValue.Identifier, x.Value.Identifier);
                Assert.Equal(context.CorrelationId, x.CorrelationId);
                Assert.Equal("Initial Failure", x.FailureReason);
            },
            x => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public async Task Execute_State_Async()
    {
        var value = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(value);

        var block = new ReturnContext_AsyncCodeBlock();

        var result = await block.ExecuteAsync(context);

        result.Value.Match(
            x => Assert.Fail(x.FailureReason ?? "Empty FailureReason"),
            x => Assert.Equal(value.Identifier, x.Identifier));
    }

    [Fact]
    public async Task Execute_Fail_State_Async()
    {
        var value = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(value);

        var block = new ReturnFailContext_AsyncCodeBlock();

        var result = await block.ExecuteAsync(context);

        result.Value.Match(
            x =>
            {
                Assert.Equal(value.Identifier, x.Value.Identifier);
                Assert.Equal(context.CorrelationId, x.CorrelationId);
                Assert.Equal("Intentional", x.FailureReason);
            },
            x => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public async Task Execute_Exception_State_Async()
    {
        var value = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(value);

        var block = new Exception_AsyncCodeBlock();

        try
        {
            var result = await block.ExecuteAsync(context);
            Assert.Fail("Expected exception");
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }
    }
}
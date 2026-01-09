using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Test;

public class CodeBlockTests
{
    [Fact]
    public void Execute_WithFailContext()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);
        value.SignalBreak(new DefaultFailureState<MyValue>(initialValue)
        {
            FailureReason = "Initial Failure"
        });

        var block = new ReturnValue_CodeBlock();
        var result = block.Execute(value);

        result.Match(
            f =>
            {
                Assert.Equal(initialValue.Identifier, f.TryGetValue<MyValue>(out var fc) ? fc.Identifier : default);
                Assert.Equal(value.CorrelationId, f.CorrelationId);
                Assert.Equal("Initial Failure", f.FailureReason);
            },
            s => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public void Execute_State()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        var block = new ReturnValue_CodeBlock();

        var result = block.Execute(value);

        result.Match(
            f => Assert.Fail(f.FailureReason ?? "Empty FailureReason"),
            s => Assert.Equal(value.Value.Identifier, s.Identifier));
    }

    [Fact]
    public void Execute_Fail_State()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        var block = new ReturnFailContext_CodeBlock();

        var result = block.Execute(value);

        Assert.Equal(value.CorrelationId, result.CorrelationId);

        result.Match(
            f =>
            {
                Assert.Equal(value.Value.Identifier, f.TryGetValue<MyValue>(out var fc) ? fc.Identifier : default);
                Assert.Equal(value.CorrelationId, f.CorrelationId);
                Assert.Equal("Intentional", f.FailureReason);
            },
            _ => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public void Execute_Exception_State()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        var block = new Exception_CodeBlock();

        try
        {
            var result = block.Execute(value);
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
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        value.SignalBreak(new DefaultFailureState<MyValue>(initialValue)
        {
            FailureReason = "Initial Failure"
        });
        var block = new ReturnValue_AsyncCodeBlock();

        // Act
        var result = await block.ExecuteAsync(value);

        Assert.Equal(value.CorrelationId, result.CorrelationId);

        // Assert
        result.Match(
            x =>
            {
                Assert.Equal(initialValue.Identifier, x.TryGetValue<MyValue>(out var xc) ? xc.Identifier : default);
                Assert.Equal(result.CorrelationId, x.CorrelationId);
                Assert.Equal("Initial Failure", x.FailureReason);
            },
            x => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public async Task Execute_State_Async()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        var block = new ReturnValue_AsyncCodeBlock();

        var result = await block.ExecuteAsync(value);

        result.Match(
            x => Assert.Fail(x.FailureReason ?? "Empty FailureReason"),
            x => Assert.Equal(value.Value.Identifier, x.Identifier));
    }

    [Fact]
    public async Task Execute_Fail_State_Async()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        var block = new ReturnFail_AsyncCodeBlock();

        var result = await block.ExecuteAsync(value);

        result.Match(
            x =>
            {
                Assert.Equal(value.Value.Identifier, x.TryGetValue<MyValue>(out var xc) ? xc.Identifier : default);
                Assert.Equal(value.CorrelationId, x.CorrelationId);
                Assert.Equal("Intentional", x.FailureReason);
            },
            x => Assert.Fail("Expected a failure"));
    }

    [Fact]
    public async Task Execute_Exception_State_Async()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        var block = new Exception_AsyncCodeBlock();

        try
        {
            var result = await block.ExecuteAsync(value);
            Assert.Fail("Expected exception");
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }
    }
}
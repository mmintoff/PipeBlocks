namespace MM.PipeBlocks.Test;
public class NoopBlockTests
{
    [Fact]
    public void SyncNoop()
    {
        var value = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(value);

        var block = new NoopBlock<MyContext, MyValue>();
        var result = block.Execute(context);

        Assert.Equal(context.CorrelationId, result.CorrelationId);
        result.Value.Match(
            f => Assert.Fail(f.FailureReason ?? "Empty FailureReason"),
            s => Assert.Equal(value.Identifier, s.Identifier));
    }

    [Fact]
    public async Task AsyncNoop()
    {
        var value = new MyValue { Identifier = Guid.NewGuid() };
        var context = new MyContext(value);

        var block = new NoopBlock<MyContext, MyValue>();
        var result = await block.ExecuteAsync(context);
        
        Assert.Equal(context.CorrelationId, result.CorrelationId);
        result.Value.Match(
            f => Assert.Fail(f.FailureReason ?? "Empty FailureReason"),
            s => Assert.Equal(value.Identifier, s.Identifier));
    }
}
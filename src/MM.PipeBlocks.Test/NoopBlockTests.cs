using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Test;

public class NoopBlockTests
{
    [Fact]
    public void SyncNoop()
    {
        var value = new MyValue { Identifier = Guid.NewGuid() };

        var block = new NoopBlock<MyValue>();
        var result = block.Execute(value);

        result.Match(
            f => Assert.Fail(f.FailureReason ?? "Empty FailureReason"),
            s => Assert.Equal(value.Identifier, s.Identifier));
    }

    [Fact]
    public async Task AsyncNoop()
    {
        var value = new MyValue { Identifier = Guid.NewGuid() };

        var block = new NoopBlock<MyValue>();
        var result = await block.ExecuteAsync(value);

        result.Match(
            f => Assert.Fail(f.FailureReason ?? "Empty FailureReason"),
            s => Assert.Equal(value.Identifier, s.Identifier));
    }
}
namespace MM.PipeBlocks.Test;

public class BlockExecutorTests
{
    [Fact]
    public void RunSyncOverSync()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var block = new ReturnValue_CodeBlock();

        var result = BlockExecutor.ExecuteSync(block, initialValue);
        Assert.Equal(initialValue.Identifier, result.Value.Identifier);
    }

    [Fact]
    public async Task RunSyncOverAsync()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var block = new ReturnValue_CodeBlock();

        var result = await BlockExecutor.ExecuteAsync(block, initialValue);
        Assert.Equal(initialValue.Identifier, result.Value.Identifier);
    }

    [Fact]
    public void RunAsyncOverSync()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var block = new ReturnValue_AsyncCodeBlock();

        var result = BlockExecutor.ExecuteSync(block, initialValue);
        Assert.Equal(initialValue.Identifier, result.Value.Identifier);
    }

    [Fact]
    public async Task RunAsyncOverAsync()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var block = new ReturnValue_AsyncCodeBlock();

        var result = await BlockExecutor.ExecuteAsync(block, initialValue);
        Assert.Equal(initialValue.Identifier, result.Value.Identifier);
    }
}
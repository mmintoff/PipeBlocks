//namespace MM.PipeBlocks.Test;
//public class BlockExecutorTests
//{
//    [Fact]
//    public void RunSyncOverSync()
//    {
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);
//        var block = new ReturnValue_CodeBlock();

//        var result = BlockExecutor.ExecuteSync(block, context);
//        Assert.Equal(context.CorrelationId, result.CorrelationId);
//    }

//    [Fact]
//    public async Task RunSyncOverAsync()
//    {
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);
//        var block = new ReturnValue_CodeBlock();

//        var result = await BlockExecutor.ExecuteAsync(block, context);
//        Assert.Equal(context.CorrelationId, result.CorrelationId);
//    }

//    [Fact]
//    public void RunAsyncOverSync()
//    {
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);
//        var block = new ReturnValue_AsyncCodeBlock();

//        var result = BlockExecutor.ExecuteSync(block, context);
//        Assert.Equal(context.CorrelationId, result.CorrelationId);
//    }

//    [Fact]
//    public async Task RunAsyncOverAsync()
//    {
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);
//        var block = new ReturnValue_AsyncCodeBlock();

//        var result = await BlockExecutor.ExecuteAsync(block, context);
//        Assert.Equal(context.CorrelationId, result.CorrelationId);
//    }
//}